import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatQueryRequest, ChatQueryResponse, InterviewQuestionResponse } from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly baseUrl = `${environment.apiBaseUrl}/chat`;
  private readonly interviewUrl = `${environment.apiBaseUrl}/interview`;

  constructor(private http: HttpClient) {}

  query(request: ChatQueryRequest): Observable<ChatQueryResponse> {
    return this.http.post<ChatQueryResponse>(`${this.baseUrl}/query`, request);
  }

  submitFeedback(chatHistoryId: string, isHelpful: boolean): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/feedback`, { chatHistoryId, isHelpful });
  }

  generateInterviewQuestions(resumeId: string): Observable<InterviewQuestionResponse> {
    return this.http.post<InterviewQuestionResponse>(`${this.interviewUrl}/generate`, { resumeId });
  }
}
