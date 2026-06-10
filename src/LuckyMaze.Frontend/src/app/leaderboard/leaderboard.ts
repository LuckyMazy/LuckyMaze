import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideTrophy, lucideMedal, lucideCoins, lucideArrowLeft } from '@ng-icons/lucide';
import { RouterLink } from '@angular/router';
import { UserService } from '../api/api/user.service';
import { LeaderboardEntryDto } from '../api/model/leaderboardEntryDto';
import { ContentHeader } from '../shared/components/content-header/content-header';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'luckymaze-leaderboard',
  standalone: true,
  imports: [CommonModule, NgIcon, RouterLink, ContentHeader],
  providers: [
    provideIcons({
      lucideTrophy,
      lucideMedal,
      lucideCoins,
      lucideArrowLeft
    })
  ],
  templateUrl: './leaderboard.html'
})
export class LeaderboardComponent implements OnInit {
  private readonly userService = inject(UserService);

  protected readonly leaderboard = signal<LeaderboardEntryDto[]>([]);
  protected readonly isLoading = signal<boolean>(true);

  async ngOnInit(): Promise<void> {
    try {
      const data = await firstValueFrom(this.userService.apiUserLeaderboardGet(15));
      this.leaderboard.set(data || []);
    } catch (err) {
      console.error('Failed to load leaderboard:', err);
    } finally {
      this.isLoading.set(false);
    }
  }
}
