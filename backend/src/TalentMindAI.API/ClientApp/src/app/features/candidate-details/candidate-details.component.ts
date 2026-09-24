import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { NavShellComponent } from '../../shared/components/nav-shell/nav-shell.component';
import { ResumeService } from '../../core/services/resume.service';
import { ChatService } from '../../core/services/chat.service';
import { CandidateDetails } from '../../core/models/resume.models';
import { InterviewQuestion } from '../../core/models/chat.models';

@Component({
  selector: 'tma-candidate-details',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatTabsModule, NavShellComponent],
  templateUrl: './candidate-details.component.html',
  styleUrl: './candidate-details.component.scss'
})
export class CandidateDetailsComponent implements OnInit {
  candidate = signal<CandidateDetails | null>(null);
  interviewQuestions = signal<InterviewQuestion[]>([]);
  generatingQuestions = signal(false);

  constructor(private route: ActivatedRoute, private resumeService: ResumeService, private chatService: ChatService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.resumeService.getDetails(id).subscribe((res) => this.candidate.set(res));
  }

  generateQuestions(): void {
    const id = this.candidate()?.resumeId;
    if (!id) return;

    this.generatingQuestions.set(true);
    this.chatService.generateInterviewQuestions(id).subscribe({
      next: (res) => {
        this.interviewQuestions.set(res.questions);
        this.generatingQuestions.set(false);
      },
      error: () => this.generatingQuestions.set(false)
    });
  }
}
