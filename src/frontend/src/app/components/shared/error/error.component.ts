import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
})
export class ErrorComponent implements OnInit {

  error: string;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.route.queryParams.subscribe(params => {
      this.error = params['error'];
    });
  }

  ngOnInit() {
    // setTimeout(() => {
    //     this.router.navigate(['/']);
    // }, 5000);  //5s
  }
}
