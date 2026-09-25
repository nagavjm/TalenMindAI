// Models for TalentMind NutriAI (Food module). Fully separate from Resume/Chat models.

export interface FoodChatQueryRequest {
  question: string;
}

export interface FoodChatQueryResponse {
  chatHistoryId: string;
  answer: string;
  sourceChunks: string[];
}

export interface RecipeDto {
  id: string;
  title: string;
  ingredients: string[];
  instructions: string;
  createdAt: string;
}

export interface RecipeListResponse {
  recipes: RecipeDto[];
}

export interface RecipeGenerateRequest {
  prompt: string;
  ingredients?: string[] | null;
  cuisine?: string | null;
  dietaryTags?: string[] | null;
  servings?: number | null;
}

export interface NutritionAnalysisDto {
  id: string;
  foodDescription: string;
  calories: number | null;
  proteinGrams: number | null;
  carbsGrams: number | null;
  fatGrams: number | null;
  createdAt: string;
}

export interface NutritionAnalyzeRequest {
  foodDescription: string;
}

export interface MealPlanDto {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  planJson: string;
  notes?: string | null;
  generatedByAi: boolean;
  createdAt: string;
}

export interface MealPlanListResponse {
  plans: MealPlanDto[];
}

export interface MealPlanGenerateRequest {
  title: string;
  startDate: string;
  endDate: string;
  dietaryTags?: string[] | null;
  notes?: string | null;
}
