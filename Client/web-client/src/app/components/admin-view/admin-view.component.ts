import { Component, OnInit } from '@angular/core';
import { MoodTrackerService, MoodEntry } from '../../services/mood-tracker.service';

@Component({
  selector: 'app-admin-view',
  templateUrl: './admin-view.component.html',
  styleUrls: ['./admin-view.component.scss']
})
export class AdminViewComponent implements OnInit {
  adminKey: string = '';
  entries: MoodEntry[] = [];
  hasLoaded = false;
  errorMessage: string | null = null;
  isLoading = false;

  constructor(private moodTrackerService: MoodTrackerService) { }

  ngOnInit(): void { }

  loadEntries(): void {
    if (!this.adminKey.trim()) {
      this.errorMessage = 'Please enter the admin key.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.moodTrackerService.getAllMoodsForAdmin(this.adminKey.trim()).subscribe({
      next: (data) => {
        this.entries = data;
        this.hasLoaded = true;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.status === 401
          ? 'Invalid admin key.'
          : 'Something went wrong loading mood entries.';
        this.isLoading = false;
      }
    });
  }

  ratingLabel(rating: number): string {
    const labels: Record<number, string> = {
      1: 'Not good at all',
      2: 'A bit "meh"',
      3: 'Pretty good',
      4: 'Feeling great'
    };
    return labels[rating] ?? 'Unknown';
  }
}