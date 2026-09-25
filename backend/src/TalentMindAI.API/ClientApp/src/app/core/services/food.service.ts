import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FoodChatQueryRequest,
  FoodChatQueryResponse,
  RecipeDto,
  RecipeListResponse,
  RecipeGenerateRequest,
  NutritionAnalysisDto,
  NutritionAnalyzeRequest,
  MealPlanDto,
  MealPlanListResponse,
  MealPlanGenerateRequest
} from '../models/food.models';

/**
 * Dedicated client for TalentMind NutriAI (Food module) APIs.
 * Isolated from ChatService/ResumeService - never reuses Resume Assistant endpoints.
 */
@Injectable({ providedIn: 'root' })
export class FoodService {
  private readonly baseUrl = `${environment.apiBaseUrl}/food`;

  constructor(private http: HttpClient) {}

  queryChat(request: FoodChatQueryRequest): Observable<FoodChatQueryResponse> {
    return this.http.post<FoodChatQueryResponse>(`${this.baseUrl}/chat/query`, request);
  }

  submitChatFeedback(chatHistoryId: string, isHelpful: boolean): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/chat/feedback`, { chatHistoryId, isHelpful });
  }

  getRecipes(): Observable<RecipeListResponse> {
    return this.http.get<RecipeListResponse>(`${this.baseUrl}/recipes`);
  }

  generateRecipe(request: RecipeGenerateRequest): Observable<RecipeDto> {
    return this.http.post<RecipeDto>(`${this.baseUrl}/recipes/generate`, request);
  }

  getNutritionHistory(): Observable<NutritionAnalysisDto[]> {
    return this.http.get<NutritionAnalysisDto[]>(`${this.baseUrl}/nutrition/history`);
  }

  analyzeNutrition(request: NutritionAnalyzeRequest): Observable<NutritionAnalysisDto> {
    return this.http.post<NutritionAnalysisDto>(`${this.baseUrl}/nutrition/analyze`, request);
  }

  getMealPlans(): Observable<MealPlanListResponse> {
    return this.http.get<MealPlanListResponse>(`${this.baseUrl}/meal-planner`);
  }

  generateMealPlan(request: MealPlanGenerateRequest): Observable<MealPlanDto> {
    return this.http.post<MealPlanDto>(`${this.baseUrl}/meal-planner/generate`, request);
  }
}
