import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../../shared/components/nav-shell/nav-shell.component';
import { FoodService } from '../../../core/services/food.service';

interface FoodChatMessage {
  role: 'user' | 'assistant';
  text: string;
  chatHistoryId?: string;
}

/**
 * Nutrition chat experience for TalentMind NutriAI. Fully separate from /chat (Resume Assistant).
 */
@Component({
  selector: 'tma-food-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './food-chat.component.html',
  styleUrl: './food-chat.component.scss'
})
export class FoodChatComponent {
  messages = signal<FoodChatMessage[]>([]);
  question = '';
  sending = signal(false);

  constructor(private foodService: FoodService) {}

  send(): void {
    const question = this.question.trim();
    if (!question) return;

    this.messages.update((m) => [...m, { role: 'user', text: question }]);
    this.question = '';
    this.sending.set(true);

    this.foodService.queryChat({ question }).subscribe({
      next: (res) => {
        this.messages.update((m) => [...m, { role: 'assistant', text: res.answer, chatHistoryId: res.chatHistoryId }]);
        this.sending.set(false);
      },
      error: () => {
        this.messages.update((m) => [...m, { role: 'assistant', text: 'Sorry, something went wrong.' }]);
        this.sending.set(false);
      }
    });
  }

  feedback(chatHistoryId: string | undefined, isHelpful: boolean): void {
    if (!chatHistoryId) return;
    this.foodService.submitChatFeedback(chatHistoryId, isHelpful).subscribe();
  }
}
