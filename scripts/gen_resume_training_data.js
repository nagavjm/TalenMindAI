const fs = require("fs");

let seed = 42;
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

function makeResume(candidateRole, industry) {
  const name = `${choice(FIRST_NAMES)} ${choice(LAST_NAMES)}`;
  const years = randInt(1, 20);
  const company = choice(COMPANY_TYPES[industry]);
  const candTech = pickTech(4, 10);
  const certs = sample(CERTS, randInt(0, 2));
  const lines = [
    `${name} - ${candidateRole} with ${years} years of experience.`,
    `Currently working at ${company} as a ${candidateRole}.`,
    `Technical skills: ${candTech.join(", ")}.`
  ];
  if (certs.length) lines.push(`Certifications: ${certs.join(", ")}.`);
  if (["Technical Lead", "Engineering Manager", "Solution Architect"].includes(candidateRole)) {
    lines.push("Led teams of 5-10 engineers, drove architecture decisions, and mentored junior developers.");
  } else if (["Senior Software Engineer", "Senior .NET Developer", "Azure Developer", "Full Stack Developer"].includes(candidateRole)) {
    lines.push("Designed and implemented scalable backend services and contributed to architecture discussions.");
  } else {
    lines.push("Contributed to feature development, bug fixes, and unit testing under senior guidance.");
  }
  lines.push("Experience delivering production systems in agile teams with CI/CD pipelines.");
  return { resumeText: lines.join(" "), candTech, years, certs };
}

function classify(score) {
  if (score >= 80) return "Strong Match";
  if (score >= 50) return "Medium Match";
  return "Poor Match";
}

function buildRecord(targetBucket) {
  const jdRole = choice(ROLES);
  const industry = choice(INDUSTRIES);
  const { jdText, reqTech, yearsReq, certReq } = makeJD(jdRole, industry);

  let candRole, score, overlap;
  if (targetBucket === "strong") {
    candRole = jdRole;
    score = randInt(80, 97);
    overlap = uniform(0.75, 1.0);
  } else if (targetBucket === "medium") {
    candRole = choice(ROLES);
    score = randInt(50, 79);
    overlap = uniform(0.4, 0.7);
  } else {
    candRole = choice(ROLES);
    score = randInt(10, 49);
    overlap = uniform(0.05, 0.35);
  }

  const { resumeText, candTech, years, certs } = makeResume(candRole, industry);

  const reqSet = new Set(reqTech);
  const candSet = new Set(candTech);
  const nMatch = Math.max(1, Math.floor(reqSet.size * overlap));
  let matched = reqTech.filter(t => candSet.has(t));
  if (matched.length < nMatch) {
    const extra = reqTech.filter(t => !matched.includes(t));
    matched = matched.concat(extra.slice(0, Math.max(0, nMatch - matched.length)));
  }
  matched = Array.from(new Set(matched)).slice(0, nMatch);
  const missing = reqTech.filter(t => !matched.includes(t));

  const classification = classify(score);
  const techScore = Math.max(0, Math.min(100, score + randInt(-8, 8)));
  const hasCloud = reqTech.some(t => t.startsWith("Azure") || t === "AWS");
  const cloudScore = Math.max(0, Math.min(100, hasCloud ? score + randInt(-15, 10) : score - randInt(10, 30)));
  let leadershipScore;
  const leaderRoles = ["Technical Lead", "Engineering Manager", "Solution Architect"];
  if (leaderRoles.includes(jdRole)) {
    leadershipScore = leaderRoles.includes(candRole)
      ? Math.max(0, Math.min(100, score + randInt(-10, 5)))
      : Math.max(0, score - randInt(20, 40));
  } else {
    leadershipScore = randInt(0, 40);
  }

  const strengths = [];
  if (matched.length) strengths.push(`Strong hands-on experience with ${matched.slice(0, 3).join(", ")}`);
  if (years >= yearsReq) strengths.push(`${years} years of experience meets or exceeds the ${yearsReq}+ year requirement`);
  if (certs.length) strengths.push(`Holds relevant certification(s): ${certs.join(", ")}`);
  if (candRole === jdRole) strengths.push(`Current role as ${candRole} directly aligns with the target position`);
  if (!strengths.length) strengths.push("Demonstrates general software development experience");

  const gaps = [];
  if (missing.length) gaps.push(`Lacks demonstrated experience with ${missing.slice(0, 3).join(", ")}`);
  if (years < yearsReq) gaps.push(`Has ${years} years of experience versus the ${yearsReq}+ years required`);
  if (certReq && !certs.includes(certReq)) gaps.push(`Missing preferred certification ${certReq}`);
  if (leaderRoles.includes(jdRole) && !leaderRoles.includes(candRole)) gaps.push("Limited evidence of leadership or architectural ownership");
  if (!gaps.length) gaps.push("No significant gaps identified");

  const riskFactors = [];
  if (classification === "Poor Match") riskFactors.push("Significant skill and experience mismatch with job requirements");
  if (years < Math.max(1, yearsReq - 3)) riskFactors.push("Experience level notably below job requirement, may require extended ramp-up");
  if (!matched.length) riskFactors.push("No overlap in core required technologies");
  if (!riskFactors.length) riskFactors.push("No major risk factors identified");

  const interviewFocus = [];
  if (missing.length) interviewFocus.push(`Assess depth of knowledge or willingness to learn ${missing[0]}`);
  interviewFocus.push("Validate hands-on coding ability with a technical exercise or pairing session");
  if (leaderRoles.includes(jdRole)) interviewFocus.push("Probe leadership style, mentoring approach, and architectural decision-making");
  interviewFocus.push("Discuss past project ownership and collaboration with cross-functional teams");

  let recommendation, recommendedLevel;
  if (classification === "Strong Match") {
    recommendation = "Proceed to technical interview; candidate is well-aligned with the role requirements.";
    recommendedLevel = jdRole;
  } else if (classification === "Medium Match") {
    recommendation = "Consider for interview with focus on closing identified skill gaps; may fit a slightly adjusted level.";
    recommendedLevel = candRole;
  } else {
    recommendation = "Not recommended for this role at this time; significant gaps outweigh current fit.";
    recommendedLevel = candRole;
  }

  const response = {
    matchScore: score,
    classification,
    yearsOfExperience: years,
    currentRoleFit: candRole,
    technicalSkillScore: techScore,
    cloudSkillScore: cloudScore,
    leadershipScore,
    matchedSkills: matched,
    missingSkills: missing,
    strengths,
    gaps,
    riskFactors,
    interviewFocusAreas: interviewFocus,
    recommendation,
    recommendedLevel
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
  const nTotal = 5000;
  const nStrong = Math.floor(nTotal * 0.4);
  const nMedium = Math.floor(nTotal * 0.4);
  const nPoor = nTotal - nStrong - nMedium;

  let buckets = [];
  for (let i = 0; i < nStrong; i++) buckets.push("strong");
  for (let i = 0; i < nMedium; i++) buckets.push("medium");
  for (let i = 0; i < nPoor; i++) buckets.push("poor");
  // shuffle
  for (let i = buckets.length - 1; i > 0; i--) {
    const j = randInt(0, i);
    [buckets[i], buckets[j]] = [buckets[j], buckets[i]];
  }

  const seen = new Set();
  const records = [];
  for (const b of buckets) {
    let rec, attempts = 0;
    do {
      rec = buildRecord(b);
      attempts++;
    } while (seen.has(rec.messages[1].content) && attempts < 20);
    seen.add(rec.messages[1].content);
    records.push(rec);
  }

  const out = records.map(r => JSON.stringify(r)).join("\n") + "\n";
  fs.writeFileSync("resume_screening_training_data.jsonl", out, "utf-8");
  console.log("wrote", records.length, "records");
}

main();
