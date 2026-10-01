const fs = require("fs");
const src = fs.readFileSync("scripts/validate_and_split.js", "utf-8");
const noMain = src.replace(/\nmain\(\);\s*$/, "");
const fn = new Function("require", noMain + "\nreturn { auditRecord };");
const { auditRecord } = fn(require);

const fn2 = new Function("require", noMain + "\nreturn { extractSkills, jdSkillSection, resumeSection };");
const { extractSkills, jdSkillSection, resumeSection } = fn2(require);

const lines = fs.readFileSync("resume_screening_training_data_incomplete.jsonl", "utf-8").trim().split("\n");
let shown = 0;
for (const line of lines) {
  const rec = JSON.parse(line);
  const a = auditRecord(rec);
  if (a.errors.length) {
    const user = rec.messages[1].content;
    const resp = JSON.parse(rec.messages[2].content);
    const jdSkills = extractSkills(jdSkillSection(user));
    const resumeSkills = extractSkills(resumeSection(user));
    console.log(JSON.stringify(a.errors));
    console.log("JD section:", jdSkillSection(user));
    console.log("jdSkills:", jdSkills);
    console.log("resumeSkills:", resumeSkills);
    console.log("resp.matchedSkills:", resp.matchedSkills, "resp.missingSkills:", resp.missingSkills, "resp.skillMatchPercentage:", resp.skillMatchPercentage);
    console.log("---");
    shown++;
    if (shown >= 3) break;
  }
}
