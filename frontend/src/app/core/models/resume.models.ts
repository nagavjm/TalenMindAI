export interface ResumeUploadResponse {
  resumeId: string;
  fileName: string;
  status: string;
}

export interface ResumeSummary {
  id: string;
  candidateName: string;
  fileName: string;
  status: string;
  createdAt: string;
}

export interface ResumeAnalysis {
  resumeId: string;
  candidateSummary: string;
  technicalSkills: string[];
  certifications: string[];
  yearsOfExperience: number;
  strengths: string[];
  suggestedRoles: string[];
  keyPhrases?: string[];
  namedEntities?: string[];
}

export interface CandidateDetails {
  resumeId: string;
  candidateName: string;
  fileName: string;
  status: string;
  analysis: ResumeAnalysis | null;
}
