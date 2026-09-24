import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { ChatService } from '../../core/services/chat.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  chatHistoryId?: string;
}

@Component({
  selector: 'tma-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss'
})
export class AiChatComponent {
  messages = signal<ChatMessage[]>([]);
  question = '';
  sending = signal(false);

  constructor(private chatService: ChatService) {}

  send(): void {
    const question = this.question.trim();
    if (!question) return;

    this.messages.update((m) => [...m, { role: 'user', text: question }]);
    this.question = '';
    this.sending.set(true);

    this.chatService.query({ question }).subscribe({
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
    this.chatService.submitFeedback(chatHistoryId, isHelpful).subscribe();
  }
}
