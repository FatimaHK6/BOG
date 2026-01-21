import { Component, ViewChild, OnInit } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'نظام قيد الدعاوى';
  showSidenav = true;

  @ViewChild('sidenav') sidenav!: MatSidenav;

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      // Hide sidenav on requests list page
      this.showSidenav = !event.url.includes('/case-registration/requests');
    });

    // Check initial route
    this.showSidenav = !this.router.url.includes('/case-registration/requests');
  }
}
