// Strict validator + train/validation/test splitter for resume screening JSONL datasets.
const fs = require("fs");

const FILES = ["resume_screening_training_data.jsonl", "resume_screening_training_data_incomplete.jsonl"];
const TECH = ["C#", ".NET Framework", ".NET Core", ".NET 8", "ASP.NET Core", "Web API",
  "SQL Server", "Azure", "Azure Functions", "Azure Service Bus", "Azure Storage",
  "Azure DevOps", "Angular", "React", "JavaScript", "TypeScript", "Docker",
  "Kubernetes", "Microservices", "Redis", "Elasticsearch", "Python", "Java", "AWS"];
const CERTS = ["AZ-204", "AZ-305", "AI-102", "AZ-900", "AWS Certified Developer"];
const decisionMap = { "Strong Match": "Shortlist", "Medium Match": "Hold", "Poor Match": "Reject" };

function classify(score) {
  if (score >= 80) return "Strong Match";
  if (score >= 50) return "Medium Match";
  return "Poor Match";
}

// Extract skills explicitly present in a text blob using word-boundary-safe matching,
// sorted by length descending so longer names (".NET Core") match before substrings (".NET").
function extractSkills(text) {
  const found = new Set();
  let masked = text;
  // Longest-name-first so compound skills ("Azure Service Bus") are matched and
  // consumed before their shorter prefixes ("Azure") get a chance to match the
  // leftover fragment, which previously caused phantom "Azure" false positives.
  const sorted = [...TECH].sort((a, b) => b.length - a.length);
  for (const skill of sorted) {
    const escaped = skill.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    // Require non-alphanumeric (or string boundary) on both sides so "Java" doesn't
    // match inside "JavaScript" and ".NET" doesn't match inside ".NET Core".
    const re = new RegExp(`(?<![A-Za-z0-9])${escaped}(?![A-Za-z0-9])`, "g");
    if (re.test(masked)) {
      found.add(skill);
      masked = masked.replace(new RegExp(`(?<![A-Za-z0-9])${escaped}(?![A-Za-z0-9])`, "g"), " ".repeat(skill.length));
    }
  }
  return Array.from(found);
}

function jdSkillSection(userText) {
  const m = userText.match(/Must have strong hands-on experience with: ([\s\S]+?)\.\s+(?:Preferred certification|Candidate must|Responsibilities include)/);
  return m ? m[1] : "";
}
function resumeSection(userText) {
  // Only the explicit "Technical skills:" list counts as declared resume skills —
  // scanning the full resume prose picks up incidental mentions (e.g. "Azure" inside
  // a sentence about cloud migration) that were never listed as a candidate skill.
  const m = userText.match(/Technical skills[^:]*:\s*([\s\S]+?)\.\s+(?:Certifications|Designed|Experience|Led|Contributed|Has direct|Provided|Notice)/);
  return m ? m[1] : "";
}
function certSection(userText) {
  const m = userText.match(/Preferred certification: ([\w-]+)/);
  return m ? m[1] : null;
}

function auditRecord(rec) {
  const errors = [];
  const user = rec.messages[1].content;
  const resp = JSON.parse(rec.messages[2].content);

  const jdSkills = extractSkills(jdSkillSection(user));
  const resumeText = resumeSection(user);
  const resumeSkills = extractSkills(resumeText);
  const resumeSet = new Set(resumeSkills);

  const trueMatched = jdSkills.filter(s => resumeSet.has(s));
  const trueMissing = jdSkills.filter(s => !resumeSet.has(s));

  const hallucinated = (resp.matchedSkills || []).filter(s => !resumeSet.has(s) || !jdSkills.includes(s));
  if (hallucinated.length) errors.push(`hallucinated matchedSkills: ${hallucinated.join(", ")}`);

  const badMissing = (resp.missingSkills || []).filter(s => resumeSet.has(s) || !jdSkills.includes(s));
  if (badMissing.length) errors.push(`invalid missingSkills: ${badMissing.join(", ")}`);

  const sortedEq = (a, b) => JSON.stringify([...a].sort()) === JSON.stringify([...b].sort());
  if (!sortedEq(resp.matchedSkills || [], trueMatched)) errors.push("matchedSkills != true intersection");
  if (!sortedEq(resp.missingSkills || [], trueMissing)) errors.push("missingSkills != JD-matched");

  [resp.matchedSkills, resp.missingSkills, resp.resumeSkills].forEach((arr, i) => {
    if (arr && new Set(arr).size !== arr.length) errors.push(`duplicate values in array index ${i}`);
  });

  const expectedPct = jdSkills.length ? Math.round((trueMatched.length / jdSkills.length) * 100) : 0;
  if (resp.skillMatchPercentage === undefined) errors.push("missing skillMatchPercentage");
  else if (resp.skillMatchPercentage !== expectedPct) errors.push(`skillMatchPercentage mismatch (${resp.skillMatchPercentage} vs ${expectedPct})`);

  if (resp.classification !== classify(resp.matchScore)) errors.push("classification/matchScore mismatch");

  if (resp.hiringDecision === undefined) errors.push("missing hiringDecision");
  else if (decisionMap[resp.classification] !== resp.hiringDecision) errors.push("hiringDecision/classification mismatch");

  const rtext = resp.recommendation || "";
  if (resp.classification === "Strong Match" && !/^Proceed/i.test(rtext)) errors.push("recommendation mismatch (Strong)");
  if (resp.classification === "Medium Match" && !/^Hold/i.test(rtext)) errors.push("recommendation mismatch (Medium)");
  if (resp.classification === "Poor Match" && !/^Reject/i.test(rtext)) errors.push("recommendation mismatch (Poor)");

  if (resp.certificationMatch === undefined) errors.push("missing certificationMatch");
  const certReq = certSection(user);
  const fullResumeText = user.slice(user.indexOf("Resume:"));
  if (certReq && resp.certificationMatch === true && !fullResumeText.includes(certReq)) errors.push("certificationMatch=true but cert not in resume");

  if (resp.relevantExperienceYears != null && resp.yearsOfExperience != null && resp.relevantExperienceYears > resp.yearsOfExperience) {
    errors.push("relevantExperienceYears exceeds yearsOfExperience");
  }

  if (resp.trainingReady !== true) errors.push("trainingReady not true");

  const deduction = errors.length * 12;
  const overallScore = Math.max(0, 100 - deduction);
  return { errors, overallScore, trainingReady: errors.length === 0 };
}

function main() {
  let all = [];
  for (const f of FILES) {
    if (!fs.existsSync(f)) continue;
    const lines = fs.readFileSync(f, "utf-8").trim().split("\n");
    lines.forEach(line => all.push({ file: f, rec: JSON.parse(line) }));
  }
  console.log("Total records loaded:", all.length);

  let kept = [], rejected = 0;
  const errorTally = {};
  for (const { file, rec } of all) {
    const audit = auditRecord(rec);
    if (audit.trainingReady && audit.overallScore >= 90) {
      kept.push(rec);
    } else {
      rejected++;
      audit.errors.forEach(e => {
        const key = e.split(" (")[0].split(":")[0];
        errorTally[key] = (errorTally[key] || 0) + 1;
      });
    }
  }

  console.log("Kept (trainingReady && overallScore>=90):", kept.length);
  console.log("Rejected:", rejected);
  console.log("Top rejection reasons:", errorTally);

  // Deterministic shuffle
  let seed = 42;
  function rand() { seed = (seed * 1103515245 + 12345) & 0x7fffffff; return seed / 0x7fffffff; }
  for (let i = kept.length - 1; i > 0; i--) {
    const j = Math.floor(rand() * (i + 1));
    [kept[i], kept[j]] = [kept[j], kept[i]];
  }

  const nTrain = Math.floor(kept.length * 0.8);
  const nVal = Math.floor(kept.length * 0.1);
  const train = kept.slice(0, nTrain);
  const validation = kept.slice(nTrain, nTrain + nVal);
  const test = kept.slice(nTrain + nVal);

  fs.writeFileSync("train.jsonl", train.map(r => JSON.stringify(r)).join("\n") + "\n");
  fs.writeFileSync("validation.jsonl", validation.map(r => JSON.stringify(r)).join("\n") + "\n");
  fs.writeFileSync("test.jsonl", test.map(r => JSON.stringify(r)).join("\n") + "\n");

  console.log("train.jsonl:", train.length, "validation.jsonl:", validation.length, "test.jsonl:", test.length);
}

main();
