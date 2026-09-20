import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SessionService } from '../auth/session.service';
import { ProductList } from '../products/product-list/product-list';
import { Register } from '../register/register';

// The front page: a welcome, the sign-up form on demand, and the whole
// catalogue underneath, for members and visitors alike.
@Component({
  selector: 'app-home',
  imports: [ProductList, Register, RouterLink],
  templateUrl: './home.html',
})
export class Home {
  readonly signedIn = inject(SessionService).signedIn;
  readonly registering = signal(false);
}
