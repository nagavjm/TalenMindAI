import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { NavShellComponent } from '../../../shared/components/nav-shell/nav-shell.component';
import { FoodService } from '../../../core/services/food.service';

interface FoodChatMessage {
  role: 'user' | 'assistant';
  text: string;
  chatHistoryId?: string;
  html?: SafeHtml;
  imageUrl?: string | null;
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

  constructor(private foodService: FoodService, private sanitizer: DomSanitizer) {}

  send(): void {
    const question = this.question.trim();
    if (!question) return;

    this.messages.update((m) => [...m, { role: 'user', text: question, html: this.toHtml(question) }]);
    this.question = '';
    this.sending.set(true);

    this.foodService.queryChat({ question }).subscribe({
      next: (res) => {
        this.messages.update((m) => [
          ...m,
          {
            role: 'assistant',
            text: res.answer,
            chatHistoryId: res.chatHistoryId,
            html: this.toHtml(res.answer),
            imageUrl: res.imageUrl
          }
        ]);
        this.sending.set(false);
      },
      error: () => {
        this.messages.update((m) => [
          ...m,
          { role: 'assistant', text: 'Sorry, something went wrong.', html: this.toHtml('Sorry, something went wrong.') }
        ]);
        this.sending.set(false);
      }
    });
  }

  feedback(chatHistoryId: string | undefined, isHelpful: boolean): void {
    if (!chatHistoryId) return;
    this.foodService.submitChatFeedback(chatHistoryId, isHelpful).subscribe();
  }

  /**
   * Lightweight Markdown-to-HTML renderer, scoped to the Food chat only.
   * Escapes HTML first (XSS-safe), then converts a safe subset of Markdown
   * (headings, bold, italics, inline code, unordered/ordered lists, line breaks)
   * to HTML, similar to how ChatGPT/Copilot render assistant messages.
   */
  private toHtml(raw: string): SafeHtml {
    const escapeHtml = (s: string) =>
      s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    const escaped = escapeHtml(raw ?? '');
    const lines = escaped.split(/\r?\n/);
    const htmlLines: string[] = [];
    let inUl = false;
    let inOl = false;

    const closeLists = () => {
      if (inUl) { htmlLines.push('</ul>'); inUl = false; }
      if (inOl) { htmlLines.push('</ol>'); inOl = false; }
    };

    const inlineFormat = (text: string) =>
      text
        .replace(/`([^`]+)`/g, '<code>$1</code>')
        .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
        .replace(/\*([^*]+)\*/g, '<em>$1</em>');

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();

      if (!line) {
        // Only close open lists if the blank line isn't just a gap between
        // consecutive list items (AI output often separates list items with
        // blank lines, which should still render as one continuous list).
        const nextNonBlank = lines.slice(i + 1).find((l) => l.trim().length > 0)?.trim();
        const nextIsListItem = nextNonBlank && (/^[-*]\s+/.test(nextNonBlank) || /^\d+[.)]\s+/.test(nextNonBlank));
        if (!((inUl || inOl) && nextIsListItem)) {
          closeLists();
        }
        continue;
      }

      const headingMatch = line.match(/^(#{1,6})\s+(.*)$/);
      if (headingMatch) {
        closeLists();
        const level = headingMatch[1].length;
        htmlLines.push(`<h${level}>${inlineFormat(headingMatch[2])}</h${level}>`);
        continue;
      }

      const ulMatch = line.match(/^[-*]\s+(.*)$/);
      if (ulMatch) {
        if (inOl) { htmlLines.push('</ol>'); inOl = false; }
        if (!inUl) { htmlLines.push('<ul>'); inUl = true; }
        htmlLines.push(`<li>${inlineFormat(ulMatch[1])}</li>`);
        continue;
      }

      const olMatch = line.match(/^\d+[.)]\s+(.*)$/);
      if (olMatch) {
        if (inUl) { htmlLines.push('</ul>'); inUl = false; }
        if (!inOl) { htmlLines.push('<ol>'); inOl = true; }
        htmlLines.push(`<li>${inlineFormat(olMatch[1])}</li>`);
        continue;
      }

      closeLists();
      htmlLines.push(`<p>${inlineFormat(line)}</p>`);
    }

    closeLists();
    return this.sanitizer.bypassSecurityTrustHtml(htmlLines.join(''));
  }
}
