import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';

/**
 * Landing page for TalentMind NutriAI (Food module).
 * Fully separate feature area from the Resume Assistant experience.
 */
@Component({
  selector: 'tma-food-home',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, NavShellComponent],
  templateUrl: './food.component.html',
  styleUrl: './food.component.scss'
})
export class FoodComponent {}
