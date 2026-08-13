import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export enum MoodRating {
  NotGoodAtAll = 1,
  Meh = 2,
  PrettyGood = 3,
  FeelingGreat = 4
}

export interface SubmitMoodRequest {
  rating: MoodRating;
  comment?: string;
}

export interface MoodEntry {
  id: string;
  userIdentifier: string;
  rating: MoodRating;
  comment?: string;
  createdAtUtc: string;
}

@Injectable({
  providedIn: 'root'
})
export class MoodTrackerService {
  //private readonly apiUrl = 'http://localhost:5085/api/Moods';
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  submitMood(request: SubmitMoodRequest): Observable<any> {
    return this.http.post(this.apiUrl, request, { withCredentials: true });
  }

  getAllMoodsForAdmin(adminKey: string): Observable<MoodEntry[]> {
    return this.http.get<MoodEntry[]>(`${this.apiUrl}/admin`, {
      headers: { 'X-Admin-Key': adminKey }
    });
  }
}