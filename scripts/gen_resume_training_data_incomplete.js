const fs = require("fs");

let seed = 777;
function rand() {
  seed = (seed * 1103515245 + 12345) & 0x7fffffff;
  return seed / 0x7fffffff;
}
function randInt(min, max) { return Math.floor(rand() * (max - min + 1)) + min; }
function choice(arr) { return arr[randInt(0, arr.length - 1)]; }
function sample(arr, n) {
  const copy = arr.slice();
  const out = [];
  n = Math.min(n, copy.length);
  for (let i = 0; i < n; i++) {
    const idx = randInt(0, copy.length - 1);
    out.push(copy.splice(idx, 1)[0]);
  }
  return out;
}
function uniform(min, max) { return rand() * (max - min) + min; }
function chance(p) { return rand() < p; }

const TECH = ["C#", ".NET Framework", ".NET Core", ".NET 8", "ASP.NET Core", "Web API",
  "SQL Server", "Azure", "Azure Functions", "Azure Service Bus", "Azure Storage",
  "Azure DevOps", "Angular", "React", "JavaScript", "TypeScript", "Docker",
  "Kubernetes", "Microservices", "Redis", "Elasticsearch", "Python", "Java", "AWS"];

const ROLES = ["Junior Developer", "Software Engineer", "Senior Software Engineer",
  "Senior .NET Developer", "Azure Developer", "Full Stack Developer",
  "Technical Lead", "Engineering Manager", "Solution Architect"];

const CERTS = ["AZ-204", "AZ-305", "AI-102", "AZ-900", "AWS Certified Developer"];

const COMPANY_TYPES = {
  "fintech": ["a fintech payments platform", "a digital banking startup", "a fraud-detection fintech firm"],
  "healthcare": ["a healthcare technology company", "a hospital EHR software vendor", "a telehealth platform"],
  "retail": ["a large retail chain", "an e-commerce marketplace", "an omnichannel retailer"],
  "cloud-native SaaS": ["a cloud-native SaaS startup", "a multi-tenant SaaS provider", "a DevOps tooling company"],
  "consulting": ["a global IT consulting firm", "a digital transformation consultancy", "a systems integrator"],
  "enterprise product": ["an enterprise software product company", "a Fortune 500 IT division", "an ERP vendor"],
  "insurance": ["a property and casualty insurance carrier", "an insurtech company"],
  "e-commerce": ["a high-traffic e-commerce platform", "an online marketplace"],
  "logistics": ["a supply chain logistics company", "a freight management platform"],
  "media streaming": ["a video streaming service", "a digital media company"]
};
const INDUSTRIES = Object.keys(COMPANY_TYPES);

const FIRST_NAMES = ["Alex","Jordan","Sam","Priya","Wei","Carlos","Emma","Liam","Sofia","Ravi",
  "Nina","Omar","Grace","Daniel","Maria","Kenji","Aisha","Lucas","Hannah","Ivan"];
const LAST_NAMES = ["Johnson","Patel","Chen","Garcia","Smith","Kumar","Rossi","Novak","Brown","Silva",
  "Nguyen","Khan","Muller","Kim","Costa","Ivanov","Diaz","Wang","Fischer","Lopez"];

function pickTech(nMin, nMax) { return sample(TECH, randInt(nMin, nMax)); }

function makeJD(role, industry) {
  const reqTech = pickTech(5, 9);
  const yearsReq = choice([1,2,3,4,5,6,8,10,12,15]);
  const certReq = choice(CERTS.concat([null, null]));
  const company = choice(COMPANY_TYPES[industry]);
  const lines = [
    `We are hiring a ${role} for ${company}.`,
    `Required experience: ${yearsReq}+ years in software development.`,
    `Must have strong hands-on experience with: ${reqTech.join(", ")}.`
  ];
  if (certReq) lines.push(`Preferred certification: ${certReq}.`);
  if (["Technical Lead", "Engineering Manager", "Solution Architect"].includes(role)) {
    lines.push("Candidate must demonstrate leadership, mentoring, and architectural decision-making experience.");
  }
  lines.push("Responsibilities include designing scalable systems, code reviews, collaborating with cross-functional teams, and delivering high-quality production software.");
  return { jdText: lines.join(" "), reqTech, yearsReq, certReq };
}

// completeness: "complete" | "partial" | "heavy"
// baseTech: the "true" set of candidate technologies (already sized to reflect the
// intended overlap with the JD's required skills) before any missing-data filtering.
function makeResume(candidateRole, industry, completeness, baseTech) {
  const name = `${choice(FIRST_NAMES)} ${choice(LAST_NAMES)}`;
  const years = randInt(1, 20);
  const company = choice(COMPANY_TYPES[industry]);
  const candTech = baseTech;

  const missing = {
    certifications: false,
    leadership: false,
    domain: false,
    employmentHistory: false,
    currentRole: false,
    noticePeriod: true, // never stated by default; only sometimes present
    cloud: false,
    skillsPartial: false,
    summary: false,
    projectDetails: false
  };

  if (completeness === "partial") {
    const keys = ["certifications", "leadership", "domain", "employmentHistory", "cloud", "skillsPartial", "summary", "projectDetails"];
    const nMiss = randInt(1, 2);
    sample(keys, nMiss).forEach(k => missing[k] = true);
  } else if (completeness === "heavy") {
    const keys = ["certifications", "leadership", "domain", "employmentHistory", "currentRole", "cloud", "skillsPartial", "summary", "projectDetails"];
    const nMiss = randInt(4, 6);
    sample(keys, nMiss).forEach(k => missing[k] = true);
  }

  const certs = missing.certifications ? [] : sample(CERTS, randInt(0, 2));
  const noticePeriodStated = chance(0.35);

  let effectiveTech = candTech;
  if (missing.skillsPartial) effectiveTech = candTech.slice(0, Math.max(1, Math.floor(candTech.length / 2)));
  if (missing.cloud) effectiveTech = effectiveTech.filter(t => !(t.startsWith("Azure") || t === "AWS"));

  const lines = [];
  if (!missing.summary) {
    lines.push(`${name} is a results-driven professional with ${years} years of experience in software development.`);
  } else {
    lines.push(`${name} - candidate profile.`);
  }

  if (!missing.currentRole) {
    lines.push(`Currently working at ${company} as a ${candidateRole}.`);
  } else {
    lines.push(`Most recent employer not specified in resume.`);
  }

  if (!missing.employmentHistory) {
    lines.push(`Prior experience includes ${randInt(1,3)} previous roles in similar organizations over the past ${years} years.`);
  } else {
    lines.push("Detailed employment history not provided in resume.");
  }

  if (effectiveTech.length) {
    lines.push(`Technical skills listed: ${effectiveTech.join(", ")}.`);
  } else {
    lines.push("Resume does not list specific technical skills.");
  }

  if (certs.length) {
    lines.push(`Certifications: ${certs.join(", ")}.`);
  } else if (missing.certifications) {
    lines.push("No certifications mentioned in resume.");
  }

  if (!missing.leadership && ["Technical Lead", "Engineering Manager", "Solution Architect"].includes(candidateRole)) {
    lines.push("Led teams of 5-10 engineers, drove architecture decisions, and mentored junior developers.");
  } else if (missing.leadership) {
    lines.push("Resume does not mention any leadership or mentoring experience.");
  } else {
    lines.push("Contributed to feature development, bug fixes, and unit testing under senior guidance.");
  }

  if (!missing.domain) {
    lines.push(`Has direct domain experience in the ${industry} industry.`);
  } else {
    lines.push("Resume does not indicate specific industry domain experience.");
  }

  if (!missing.projectDetails) {
    lines.push("Provided details of key projects delivered, including scope, team size, and outcomes.");
  } else {
    lines.push("Resume lacks detailed project descriptions or outcomes.");
  }

  if (noticePeriodStated) {
    lines.push(`Notice period: ${choice(["Immediate", "2 weeks", "30 days", "60 days"])}.`);
  } else {
    lines.push("Notice period not mentioned.");
  }

  return { resumeText: lines.join(" "), candTech: effectiveTech, years, certs, missing, noticePeriodStated };
}

function classify(score) {
  if (score >= 80) return "Strong Match";
  if (score >= 50) return "Medium Match";
  return "Poor Match";
}

function buildRecord(targetBucket, completeness) {
  const jdRole = choice(ROLES);
  const industry = choice(INDUSTRIES);
  const { jdText, reqTech, yearsReq, certReq } = makeJD(jdRole, industry);

  let candRole, overlap;
  if (targetBucket === "strong") {
    candRole = jdRole;
    overlap = uniform(0.75, 1.0);
  } else if (targetBucket === "medium") {
    candRole = choice(ROLES);
    overlap = uniform(0.4, 0.7);
  } else {
    candRole = choice(ROLES);
    overlap = uniform(0.05, 0.35);
  }

  // Build the candidate's "true" tech list so its real intersection with reqTech
  // reflects the intended overlap for this bucket (strong/medium/poor).
  const nOverlap = Math.max(0, Math.round(reqTech.length * overlap));
  const overlapSkills = sample(reqTech, nOverlap);
  const otherTech = TECH.filter(t => !reqTech.includes(t));
  const extraSkills = sample(otherTech, randInt(1, 4));
  const baseTech = Array.from(new Set(overlapSkills.concat(extraSkills)));

  const { resumeText, candTech, years, certs, missing } = makeResume(candRole, industry, completeness, baseTech);

  // --- Rule 1-3: matchedSkills is the strict intersection of JD skills and resume skills;
  // missingSkills = JD skills minus matchedSkills. A skill can never appear in both. ---
  const candSet = new Set(candTech);
  const matched = reqTech.filter(t => candSet.has(t));
  const missingSkills = reqTech.filter(t => !candSet.has(t));

  const missingCount = Object.values(missing).filter(Boolean).length;

  // --- Rule 4: skillMatchPercentage = (matchedSkills.count / requiredSkills.count) * 100 ---
  const skillMatchPercentage = reqTech.length ? Math.round((matched.length / reqTech.length) * 100) : 0;

  // --- Component fits (neutral 50 when the underlying data is missing/unknown; never assume) ---
  const experienceFit = missing.employmentHistory
    ? 50
    : Math.max(0, Math.min(100, Math.round((years / yearsReq) * 100)));

  const certificationMatch = missing.certifications ? null : (certReq ? certs.includes(certReq) : (certs.length > 0));
  const certificationFit = missing.certifications
    ? 50
    : (certReq ? (certificationMatch ? 100 : 0) : (certs.length > 0 ? 70 : 50));

  const domainExperience = missing.domain ? null : [industry];
  const domainFit = missing.domain ? 50 : 100;

  const leaderRoles = ["Technical Lead", "Engineering Manager", "Solution Architect"];
  const leadershipScoreValue = leaderRoles.includes(jdRole)
    ? (leaderRoles.includes(candRole) ? randInt(70, 95) : randInt(10, 40))
    : randInt(0, 40);
  const leadershipScore = missing.leadership ? null : leadershipScoreValue;
  const leadershipFit = missing.leadership ? 50 : leadershipScoreValue;

  // --- Rule 5: matchScore = Skills 50% + Experience 20% + Certification 10% + Domain 10% + Leadership 10% ---
  const matchScore = Math.max(0, Math.min(100, Math.round(
    skillMatchPercentage * 0.5 +
    experienceFit * 0.2 +
    certificationFit * 0.1 +
    domainFit * 0.1 +
    leadershipFit * 0.1
  )));

  // --- Rule 6: Classification thresholds ---
  const classification = classify(matchScore);

  const techScore = candTech.length ? Math.max(0, Math.min(100, Math.round(skillMatchPercentage * 0.7 + experienceFit * 0.3))) : null;
  const hasCloudReq = reqTech.some(t => t.startsWith("Azure") || t === "AWS");
  const cloudScore = missing.cloud ? null : (hasCloudReq
    ? Math.max(0, Math.min(100, Math.round(skillMatchPercentage * 0.8 + randInt(-5, 5))))
    : Math.max(0, Math.min(100, randInt(10, 40))));

  const currentRoleFit = missing.currentRole ? null : candRole;
  const relevantExperienceYears = missing.employmentHistory ? null : years;

  const strengths = [];
  if (matched.length) strengths.push(`Strong hands-on experience with ${matched.slice(0, 3).join(", ")}`);
  if (!missing.employmentHistory && years >= yearsReq) strengths.push(`${years} years of experience meets or exceeds the ${yearsReq}+ year requirement`);
  if (certs.length) strengths.push(`Holds relevant certification(s): ${certs.join(", ")}`);
  if (currentRoleFit === jdRole) strengths.push(`Current role as ${candRole} directly aligns with the target position`);
  if (!missing.projectDetails) strengths.push("Resume includes clear project scope and delivered outcomes");
  if (!strengths.length) strengths.push("Demonstrates general software development experience");

  const gaps = [];
  if (missingSkills.length) gaps.push(`Lacks demonstrated experience with ${missingSkills.slice(0, 3).join(", ")}`);
  if (!missing.employmentHistory && years < yearsReq) gaps.push(`Has ${years} years of experience versus the ${yearsReq}+ years required`);
  if (certReq && !missing.certifications && !certs.includes(certReq)) gaps.push(`Missing preferred certification ${certReq}`);
  if (leaderRoles.includes(jdRole) && !missing.leadership && !leaderRoles.includes(candRole)) gaps.push("Limited evidence of leadership or architectural ownership");
  if (!gaps.length) gaps.push("No significant gaps identified");

  const riskFactors = [];
  if (missing.certifications) riskFactors.push("Certification information unavailable");
  if (missing.leadership) riskFactors.push("Leadership experience not mentioned");
  if (missing.domain) riskFactors.push("Domain experience not specified in resume");
  if (missing.employmentHistory) riskFactors.push("Employment history incomplete, unable to verify full work timeline");
  if (missing.currentRole) riskFactors.push("Current role not stated in resume");
  if (missing.cloud) riskFactors.push("Cloud experience not mentioned in resume");
  if (missing.skillsPartial) riskFactors.push("Resume lists only a partial set of technical skills");
  if (missing.summary) riskFactors.push("Resume summary missing, limiting overall candidate context");
  if (missing.projectDetails) riskFactors.push("Project details missing, unable to assess depth of contributions");
  if (classification === "Poor Match" && !riskFactors.length) riskFactors.push("Significant skill and experience mismatch with job requirements");
  if (!riskFactors.length) riskFactors.push("No major risk factors identified");

  const interviewFocus = [];
  if (missingSkills.length) interviewFocus.push(`Assess depth of knowledge or willingness to learn ${missingSkills[0]}`);
  interviewFocus.push("Validate hands-on coding ability with a technical exercise or pairing session");
  if (missing.leadership && leaderRoles.includes(jdRole)) interviewFocus.push("Directly probe leadership and mentoring experience not covered in resume");
  if (missing.domain) interviewFocus.push("Clarify prior industry/domain exposure during screening call");
  if (missing.certifications) interviewFocus.push("Confirm certification status and plans for future certification");
  interviewFocus.push("Discuss past project ownership and collaboration with cross-functional teams");

  // --- Rule 7: hiringDecision strictly derived from classification ---
  const HIRING_DECISION_BY_CLASSIFICATION = {
    "Strong Match": "Shortlist",
    "Medium Match": "Hold",
    "Poor Match": "Reject"
  };
  const hiringDecision = HIRING_DECISION_BY_CLASSIFICATION[classification];

  // --- Rule 8: recommendation must match classification; missing data adds a caveat, not a change in decision ---
  const missingCaveat = missingCount > 0 ? " Note: some resume details were unavailable and were not assumed." : "";
  let recommendation;
  if (classification === "Strong Match") {
    recommendation = `Proceed to Technical Interview; candidate is well-aligned with the role requirements.${missingCaveat}`;
  } else if (classification === "Medium Match") {
    recommendation = `Hold / Further Evaluation; candidate shows partial alignment and should be assessed against the identified skill and experience gaps.${missingCaveat}`;
  } else {
    recommendation = `Reject / Not Recommended; significant gaps outweigh current fit for this role.${missingCaveat}`;
  }

  const recommendedLevel = currentRoleFit || jdRole;

  const response = {
    matchScore,
    classification,
    yearsOfExperience: missing.employmentHistory ? null : years,
    relevantExperienceYears,
    skillMatchPercentage,
    technicalSkillScore: techScore,
    cloudSkillScore: cloudScore,
    leadershipScore,
    currentRoleFit,
    matchedSkills: matched,
    missingSkills,
    resumeSkills: candTech,
    certificationMatch,
    domainExperience,
    strengths,
    gaps,
    riskFactors,
    interviewFocusAreas: interviewFocus,
    recommendedLevel,
    hiringDecision,
    recommendation,
    trainingReady: true
  };

  const userContent = `Job Description: ${jdText} Resume: ${resumeText}`;

  return {
    messages: [
      { role: "system", content: "You are an expert HR recruiter, technical architect, and hiring manager who evaluates resumes against job descriptions." },
      { role: "user", content: userContent },
      { role: "assistant", content: JSON.stringify(response) }
    ]
  };
}

function main() {
  const nTotal = 13000;
  const nStrong = Math.floor(nTotal * 0.4);
  const nMedium = Math.floor(nTotal * 0.4);
  const nPoor = nTotal - nStrong - nMedium;

  const nComplete = Math.floor(nTotal * 0.7);
  const nPartial = Math.floor(nTotal * 0.2);
  const nHeavy = nTotal - nComplete - nPartial;

  let buckets = [];
  for (let i = 0; i < nStrong; i++) buckets.push("strong");
  for (let i = 0; i < nMedium; i++) buckets.push("medium");
  for (let i = 0; i < nPoor; i++) buckets.push("poor");
  for (let i = buckets.length - 1; i > 0; i--) {
    const j = randInt(0, i);
    [buckets[i], buckets[j]] = [buckets[j], buckets[i]];
  }

  let completenessArr = [];
  for (let i = 0; i < nComplete; i++) completenessArr.push("complete");
  for (let i = 0; i < nPartial; i++) completenessArr.push("partial");
  for (let i = 0; i < nHeavy; i++) completenessArr.push("heavy");
  for (let i = completenessArr.length - 1; i > 0; i--) {
    const j = randInt(0, i);
    [completenessArr[i], completenessArr[j]] = [completenessArr[j], completenessArr[i]];
  }

  const seen = new Set();
  const records = [];
  for (let i = 0; i < nTotal; i++) {
    let rec, attempts = 0;
    do {
      rec = buildRecord(buckets[i], completenessArr[i]);
      attempts++;
    } while (seen.has(rec.messages[1].content) && attempts < 20);
    seen.add(rec.messages[1].content);
    records.push(rec);
  }

  const out = records.map(r => JSON.stringify(r)).join("\n") + "\n";
  fs.writeFileSync("resume_screening_training_data_incomplete.jsonl", out, "utf-8");
  console.log("wrote", records.length, "records");
}

main();
