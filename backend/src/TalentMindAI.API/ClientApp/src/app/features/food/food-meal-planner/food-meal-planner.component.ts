import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../../shared/components/nav-shell/nav-shell.component';
import { FoodService } from '../../../core/services/food.service';
import { MealPlanDto } from '../../../core/models/food.models';

@Component({
  selector: 'tma-food-meal-planner',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './food-meal-planner.component.html',
  styleUrl: './food-meal-planner.component.scss'
})
export class FoodMealPlannerComponent implements OnInit {
  plans = signal<MealPlanDto[]>([]);
  title = '';
  startDate = '';
  endDate = '';
  dietaryPreference = '';
  generating = signal(false);

  constructor(private foodService: FoodService) {}

  ngOnInit(): void {
    this.foodService.getMealPlans().subscribe((res) => this.plans.set(res.plans));
  }

  generate(): void {
    const title = this.title.trim();
    if (!title || !this.startDate || !this.endDate) return;
    this.generating.set(true);
    const dietaryTags = this.dietaryPreference
      ? this.dietaryPreference.split(',').map((t) => t.trim()).filter(Boolean)
      : null;
    this.foodService
      .generateMealPlan({
        title,
        startDate: this.startDate,
        endDate: this.endDate,
        dietaryTags,
        notes: null
      })
      .subscribe({
        next: (plan) => {
          this.plans.update((p) => [plan, ...p]);
          this.title = '';
          this.startDate = '';
          this.endDate = '';
          this.generating.set(false);
        },
        error: () => this.generating.set(false)
      });
  }
}
