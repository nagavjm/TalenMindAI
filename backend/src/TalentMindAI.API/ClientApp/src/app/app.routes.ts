import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'resume-upload',
    canActivate: [authGuard],
    loadComponent: () => import('./features/resume-upload/resume-upload.component').then((m) => m.ResumeUploadComponent)
  },
  {
    path: 'resume-analysis/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/resume-analysis/resume-analysis.component').then((m) => m.ResumeAnalysisComponent)
  },
  {
    path: 'candidate/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/candidate-details/candidate-details.component').then((m) => m.CandidateDetailsComponent)
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/ai-chat/ai-chat.component').then((m) => m.AiChatComponent)
  },
  {
    path: 'admin-analytics',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin-analytics/admin-analytics.component').then((m) => m.AdminAnalyticsComponent)
  },
  // TalentMind NutriAI (Food module) - isolated feature routes, separate from Resume Assistant.
  {
    path: 'food',
    canActivate: [authGuard],
    loadComponent: () => import('./features/food/food.component').then((m) => m.FoodComponent)
  },
  {
    path: 'food/chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/food/food-chat/food-chat.component').then((m) => m.FoodChatComponent)
  },
  {
    path: 'food/recipes',
    canActivate: [authGuard],
    loadComponent: () => import('./features/food/food-recipes/food-recipes.component').then((m) => m.FoodRecipesComponent)
  },
  {
    path: 'food/nutrition',
    canActivate: [authGuard],
    loadComponent: () => import('./features/food/food-nutrition/food-nutrition.component').then((m) => m.FoodNutritionComponent)
  },
  {
    path: 'food/meal-planner',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/food/food-meal-planner/food-meal-planner.component').then((m) => m.FoodMealPlannerComponent)
  },
  { path: '**', redirectTo: 'login' }
];
