import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../../shared/components/nav-shell/nav-shell.component';
import { FoodService } from '../../../core/services/food.service';
import { NutritionAnalysisDto } from '../../../core/models/food.models';

@Component({
  selector: 'tma-food-nutrition',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './food-nutrition.component.html',
  styleUrl: './food-nutrition.component.scss'
})
export class FoodNutritionComponent implements OnInit {
  history = signal<NutritionAnalysisDto[]>([]);
  foodDescription = '';
  analyzing = signal(false);

  constructor(private foodService: FoodService) {}

  ngOnInit(): void {
    this.foodService.getNutritionHistory().subscribe((res) => this.history.set(res));
  }

  analyze(): void {
    const desc = this.foodDescription.trim();
    if (!desc) return;
    this.analyzing.set(true);
    this.foodService.analyzeNutrition({ foodDescription: desc }).subscribe({
      next: (result) => {
        this.history.update((h) => [result, ...h]);
        this.foodDescription = '';
        this.analyzing.set(false);
      },
      error: () => this.analyzing.set(false)
    });
  }
}
