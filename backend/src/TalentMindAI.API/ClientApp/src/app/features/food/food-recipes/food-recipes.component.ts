import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../../shared/components/nav-shell/nav-shell.component';
import { FoodService } from '../../../core/services/food.service';
import { RecipeDto } from '../../../core/models/food.models';

@Component({
  selector: 'tma-food-recipes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './food-recipes.component.html',
  styleUrl: './food-recipes.component.scss'
})
export class FoodRecipesComponent implements OnInit {
  recipes = signal<RecipeDto[]>([]);
  prompt = '';
  dietaryPreference = '';
  generating = signal(false);

  constructor(private foodService: FoodService) {}

  ngOnInit(): void {
    this.foodService.getRecipes().subscribe((res) => this.recipes.set(res.recipes));
  }

  generate(): void {
    const prompt = this.prompt.trim();
    if (!prompt) return;
    this.generating.set(true);
    const dietaryTags = this.dietaryPreference
      ? this.dietaryPreference.split(',').map((t) => t.trim()).filter(Boolean)
      : null;
    this.foodService
      .generateRecipe({ prompt, dietaryTags })
      .subscribe({
        next: (recipe) => {
          this.recipes.update((r) => [recipe, ...r]);
          this.prompt = '';
          this.generating.set(false);
        },
        error: () => this.generating.set(false)
      });
  }
}
