import { Component } from '@angular/core';
import { MoodTrackerService, MoodRating, SubmitMoodRequest } from '../../services/mood-tracker.service';

interface MoodOption {
  value: MoodRating;
  label: string;
}

@Component({
  selector: 'app-mood-form',
  templateUrl: './mood-form.component.html',
  styleUrls: ['./mood-form.component.scss']
})
export class MoodFormComponent {
  moodOptions: MoodOption[] = [
    { value: MoodRating.NotGoodAtAll, label: 'Not good at all' },
    { value: MoodRating.Meh, label: 'A bit "meh"' },
    { value: MoodRating.PrettyGood, label: 'Pretty good' },
    { value: MoodRating.FeelingGreat, label: 'Feeling great' }
  ];

  selectedRating: MoodRating | null = null;
  comment: string = '';

  isSubmitting = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(private moodTrackerService: MoodTrackerService) { }

  submit(): void {
    if (this.selectedRating === null) {
      this.errorMessage = 'Please select how you\'re feeling.';
      return;
    }

    this.isSubmitting = true;
    this.successMessage = null;
    this.errorMessage = null;

    const request: SubmitMoodRequest = {
      rating: this.selectedRating,
      comment: this.comment.trim() ? this.comment.trim() : undefined
    };

    this.moodTrackerService.submitMood(request).subscribe({
      next: () => {
        this.successMessage = 'Thanks! Your mood has been logged for today.';
        this.isSubmitting = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Something went wrong. Please try again.';
        this.isSubmitting = false;
      }
    });
  }
}