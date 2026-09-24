export interface ChatQueryRequest {
  question: string;
  resumeId?: string | null;
}

export interface ChatQueryResponse {
  chatHistoryId: string;
  answer: string;
  sourceChunks: string[];
}

export interface InterviewQuestion {
  question: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  skill: string;
}

export interface InterviewQuestionResponse {
  resumeId: string;
  questions: InterviewQuestion[];
}

export interface DashboardStats {
  totalResumes: number;
  topSkills: { skill: string; count: number }[];
  experienceDistribution: { range: string; count: number }[];
  mostPopularTechnologies: { skill: string; count: number }[];
}
