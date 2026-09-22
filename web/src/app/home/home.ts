import { Component } from '@angular/core';
import { ProductList } from '../products/product-list/product-list';

// The front page is the catalogue, for members and visitors alike.
@Component({
  selector: 'app-home',
  imports: [ProductList],
  templateUrl: './home.html',
})
export class Home {}
