import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  chatHistoryId?: string;
}

interface Conversation {
  id: string;
  title: string;
  messages: ChatMessage[];
}

@Component({
  selector: 'tma-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatIconModule, NavShellComponent],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss'
})
export class AiChatComponent {
  conversations = signal<Conversation[]>([]);
  activeConversationId = signal<string | null>(null);
  searchTerm = signal('');
  sidebarCollapsed = signal(false);

  messages = signal<ChatMessage[]>([]);
  question = '';
  sending = signal(false);

  filteredConversations = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const all = this.conversations();
    return term ? all.filter((c) => c.title.toLowerCase().includes(term)) : all;
  });

  constructor(
    private chatService: ChatService,
    public authService: AuthService,
    private router: Router
  ) {}

  toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => !v);
  }

  newConversation(): void {
    this.activeConversationId.set(null);
    this.messages.set([]);
    this.question = '';
  }

  openConversation(conversation: Conversation): void {
    this.activeConversationId.set(conversation.id);
    this.messages.set(conversation.messages);
  }

  renameConversation(conversation: Conversation, event: Event): void {
    event.stopPropagation();
    const newTitle = window.prompt('Rename chat', conversation.title);
    if (!newTitle || !newTitle.trim()) return;
    this.conversations.update((all) =>
      all.map((c) => (c.id === conversation.id ? { ...c, title: newTitle.trim() } : c))
    );
  }

  deleteConversation(conversation: Conversation, event: Event): void {
    event.stopPropagation();
    this.conversations.update((all) => all.filter((c) => c.id !== conversation.id));
    if (this.activeConversationId() === conversation.id) {
      this.newConversation();
    }
  }

  send(): void {
    const question = this.question.trim();
    if (!question) return;

    this.messages.update((m) => [...m, { role: 'user', text: question }]);
    this.question = '';
    this.sending.set(true);

    let conversationId = this.activeConversationId();
    if (!conversationId) {
      conversationId = crypto.randomUUID();
      this.activeConversationId.set(conversationId);
      this.conversations.update((c) => [
        { id: conversationId!, title: question, messages: this.messages() },
        ...c
      ]);
    }

    this.chatService.query({ question }).subscribe({
      next: (res) => {
        this.messages.update((m) => [...m, { role: 'assistant', text: res.answer, chatHistoryId: res.chatHistoryId }]);
        this.syncActiveConversation();
        this.sending.set(false);
      },
      error: () => {
        this.messages.update((m) => [...m, { role: 'assistant', text: 'Sorry, something went wrong.' }]);
        this.syncActiveConversation();
        this.sending.set(false);
      }
    });
  }

  feedback(chatHistoryId: string | undefined, isHelpful: boolean): void {
    if (!chatHistoryId) return;
    this.chatService.submitFeedback(chatHistoryId, isHelpful).subscribe();
  }

  logout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/login']));
  }

  private syncActiveConversation(): void {
    const id = this.activeConversationId();
    if (!id) return;
    this.conversations.update((all) => all.map((c) => (c.id === id ? { ...c, messages: this.messages() } : c)));
  }
}
