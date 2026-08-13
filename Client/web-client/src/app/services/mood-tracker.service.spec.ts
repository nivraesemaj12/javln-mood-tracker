import { TestBed } from '@angular/core/testing';

import { MoodTrackerService } from './mood-tracker.service';

describe('MoodTrackerService', () => {
  let service: MoodTrackerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MoodTrackerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
