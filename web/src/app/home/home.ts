import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SessionService } from '../auth/session.service';
import { ProductList } from '../products/product-list/product-list';

// The front page: a welcome and the whole catalogue underneath, for
// members and visitors alike.
@Component({
  selector: 'app-home',
  imports: [ProductList, RouterLink],
  templateUrl: './home.html',
})
export class Home {
  protected readonly signedIn = inject(SessionService).signedIn;
}
