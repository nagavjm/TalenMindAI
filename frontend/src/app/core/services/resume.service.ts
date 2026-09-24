import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CandidateDetails, ResumeAnalysis, ResumeSummary, ResumeUploadResponse } from '../models/resume.models';

@Injectable({ providedIn: 'root' })
export class ResumeService {
  private readonly baseUrl = `${environment.apiBaseUrl}/resume`;

  constructor(private http: HttpClient) {}

  upload(file: File): Observable<ResumeUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ResumeUploadResponse>(`${this.baseUrl}/upload`, formData);
  }

  analyze(resumeId: string): Observable<ResumeAnalysis> {
    return this.http.post<ResumeAnalysis>(`${this.baseUrl}/analyze`, { resumeId });
  }

  getAll(): Observable<ResumeSummary[]> {
    return this.http.get<ResumeSummary[]>(this.baseUrl);
  }

  getDetails(resumeId: string): Observable<CandidateDetails> {
    return this.http.get<CandidateDetails>(`${this.baseUrl}/${resumeId}`);
  }
}
