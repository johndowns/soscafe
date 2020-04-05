import { MediaMatcher } from '@angular/cdk/layout';
import {
  ChangeDetectorRef,
  Component,
  NgZone,
  OnDestroy,
  ViewChild,
  HostListener,
  Directive,
  AfterViewInit
} from '@angular/core';
import {
  HeaderComponent,
  SidebarComponent,
} from './../../shared';

@Component({
  selector: 'app-layout',
  templateUrl: 'default-layout.component.html'
})
export class DefaultLayoutComponent implements OnDestroy, AfterViewInit {
  mobileQuery: MediaQueryList;

  private mobileQueryListener: () => void;

  constructor(
    changeDetectorRef: ChangeDetectorRef,
    media: MediaMatcher
  ) {
    this.mobileQuery = media.matchMedia('(min-width: 768px)');
    this.mobileQueryListener = () => changeDetectorRef.detectChanges();
    this.mobileQuery.addListener(this.mobileQueryListener);
  }

  ngOnDestroy(): void {
    this.mobileQuery.removeListener(this.mobileQueryListener);
  }
  ngAfterViewInit() {}
}

