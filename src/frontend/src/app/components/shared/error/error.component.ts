import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
})
export class ErrorComponent implements OnInit {

  constructor(private router: Router) {}

  ngOnInit() {
      // do init at here for current route.

      setTimeout(() => {
          this.router.navigate(['/']);
      }, 5000);  //5s
  }
}
