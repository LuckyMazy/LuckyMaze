import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideHouse,
  lucideChevronsUpDown,
  lucideLogOut,
  lucideSun,
  lucideMoon,
  lucideMonitor,
} from '@ng-icons/lucide';
import { ThemeService, ThemeMode } from '../shared/services/theme.service';
import { HlmSidebarImports, HlmSidebarService } from '@spartan-ng/helm/sidebar';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { UserStore } from '../shared/stores/UserStore.store';

@Component({
  selector: 'luckymaze-sidenav',
  imports: [HlmSidebarImports, HlmDropdownMenuImports, HlmAvatarImports, NgIcon, RouterLink],
  providers: [
    provideIcons({
      lucideHouse,
      lucideChevronsUpDown,
      lucideLogOut,
      lucideSun,
      lucideMoon,
      lucideMonitor,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidenav.html',
})
export class Sidenav implements OnInit {
  private readonly sidebarService = inject(HlmSidebarService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly theme = inject(ThemeService);
  protected readonly userStore = inject(UserStore);

  ngOnInit(): void {
    void this.userStore.load();
  }

  protected readonly themeMode = this.theme.mode;
  protected readonly themeOptions: ReadonlyArray<{ mode: ThemeMode; label: string; icon: string }> = [
    { mode: 'light', label: 'Light', icon: 'lucideSun' },
    { mode: 'dark', label: 'Dark', icon: 'lucideMoon' },
    { mode: 'system', label: 'System', icon: 'lucideMonitor' },
  ];
  protected readonly menuSide = computed(() => (this.sidebarService.isMobile() ? 'top' : 'right'));

  protected readonly user = computed(() => {
    const u = this.userStore.currentUser();
    return {
      name: u?.displayName ?? u?.email ?? '',
      email: u?.email ?? '',
      avatar: u?.avatarUrl ?? '',
    };
  });

  protected setTheme(mode: ThemeMode): void {
    this.theme.set(mode);
  }

  protected logout(): void {
    this.oidcSecurityService
      .logoffAndRevokeTokens()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }
}
