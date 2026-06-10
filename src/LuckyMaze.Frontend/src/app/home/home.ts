import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ContentHeader } from '../shared/components/content-header/content-header';

@Component({
  selector: 'luckymaze-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContentHeader],
  templateUrl: './home.html',
})
export class Home {}
