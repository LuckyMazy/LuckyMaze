import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideArrowLeft, lucideMedal, lucideTrophy } from '@ng-icons/lucide';
import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmCard } from '@spartan-ng/helm/card';
import { HlmSpinner } from '@spartan-ng/helm/spinner';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../api/api/user.service';
import { LeaderboardEntryDto } from '../api/model/leaderboardEntryDto';
import { ContentHeader } from '../shared/components/content-header/content-header';

@Component({
  selector: 'luckymaze-leaderboard',
  imports: [
    DecimalPipe,
    RouterLink,
    NgIcon,
    HlmAvatarImports,
    HlmButton,
    HlmCard,
    HlmSpinner,
    HlmTableImports,
    ContentHeader,
  ],
  providers: [provideIcons({ lucideArrowLeft, lucideMedal, lucideTrophy })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './leaderboard.html',
})
export class LeaderboardComponent implements OnInit {
  private readonly userService = inject(UserService);

  protected readonly leaderboard = signal<LeaderboardEntryDto[]>([]);
  protected readonly isLoading = signal(true);

  async ngOnInit(): Promise<void> {
    try {
      const data = await firstValueFrom(this.userService.apiUserLeaderboardGet(15));
      this.leaderboard.set(data ?? []);
    } catch (err) {
      console.error('Failed to load leaderboard:', err);
    } finally {
      this.isLoading.set(false);
    }
  }
}
