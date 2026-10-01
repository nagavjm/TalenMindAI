const fs = require("fs");
const lines = fs.readFileSync("resume_screening_training_data_incomplete.jsonl", "utf-8").trim().split("\n");

let errors = 0;
const decisionMap = { "Strong Match": "Shortlist", "Medium Match": "Hold", "Poor Match": "Reject" };

function classify(score) {
  if (score >= 80) return "Strong Match";
  if (score >= 50) return "Medium Match";
  return "Poor Match";
}

lines.forEach((line, idx) => {
  const rec = JSON.parse(line);
  const user = rec.messages[1].content;
  const resp = JSON.parse(rec.messages[2].content);

  const jdSkillsMatch = user.match(/Must have strong hands-on experience with: ([\s\S]+?)\.\s+(?:Preferred certification|Candidate must|Responsibilities include)/);
  const reqTech = jdSkillsMatch ? jdSkillsMatch[1].split(",").map(s => s.trim()) : [];
  const resumeSkills = resp.resumeSkills || [];
  const resumeSet = new Set(resumeSkills);

  // Rule 1-3: matched = intersection, missing = req - matched, no overlap between sets
  const expectedMatched = reqTech.filter(t => resumeSet.has(t));
  const expectedMissing = reqTech.filter(t => !resumeSet.has(t));
  const overlapBad = resp.matchedSkills.some(s => resp.missingSkills.includes(s));
  if (overlapBad) { errors++; console.log(idx, "OVERLAP matched/missing"); }
  if (JSON.stringify([...resp.matchedSkills].sort()) !== JSON.stringify([...expectedMatched].sort())) {
    errors++; console.log(idx, "matchedSkills mismatch", resp.matchedSkills, expectedMatched);
  }
  if (JSON.stringify([...resp.missingSkills].sort()) !== JSON.stringify([...expectedMissing].sort())) {
    errors++; console.log(idx, "missingSkills mismatch");
  }

  // duplicates
  [resp.matchedSkills, resp.missingSkills, resp.resumeSkills].forEach((arr, i) => {
    if (new Set(arr).size !== arr.length) { errors++; console.log(idx, "duplicate in array", i); }
  });

  // skillMatchPercentage
  const expectedPct = reqTech.length ? Math.round((expectedMatched.length / reqTech.length) * 100) : 0;
  if (resp.skillMatchPercentage !== expectedPct) { errors++; console.log(idx, "skillMatchPercentage mismatch", resp.skillMatchPercentage, expectedPct); }

  // classification vs matchScore
  if (classify(resp.matchScore) !== resp.classification) { errors++; console.log(idx, "classification mismatch"); }

  // hiringDecision mapping
  if (decisionMap[resp.classification] !== resp.hiringDecision) { errors++; console.log(idx, "hiringDecision mismatch"); }

  // score ranges
  ["technicalSkillScore", "cloudSkillScore", "leadershipScore"].forEach(f => {
    const v = resp[f];
    if (v !== null && (v < 0 || v > 100)) { errors++; console.log(idx, f, "out of range", v); }
  });

  // relevantExperienceYears <= yearsOfExperience
  if (resp.relevantExperienceYears !== null && resp.yearsOfExperience !== null && resp.relevantExperienceYears > resp.yearsOfExperience) {
    errors++; console.log(idx, "relevantExperienceYears exceeds yearsOfExperience");
  }

  // recommendation aligns with classification (simple keyword check)
  const rec_ = resp.recommendation;
  if (resp.classification === "Strong Match" && !rec_.startsWith("Proceed")) { errors++; console.log(idx, "recommendation mismatch strong"); }
  if (resp.classification === "Medium Match" && !rec_.startsWith("Hold")) { errors++; console.log(idx, "recommendation mismatch medium"); }
  if (resp.classification === "Poor Match" && !rec_.startsWith("Reject")) { errors++; console.log(idx, "recommendation mismatch poor"); }

  if (resp.trainingReady !== true) { errors++; console.log(idx, "trainingReady not true"); }
});

console.log("Total records:", lines.length, "Total errors:", errors);

// distribution
const dist = {};
lines.forEach(line => {
  const resp = JSON.parse(JSON.parse(line).messages[2].content);
  dist[resp.classification] = (dist[resp.classification] || 0) + 1;
});
console.log("Distribution:", dist);
