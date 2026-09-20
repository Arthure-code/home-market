import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductList } from '../product-list/product-list';

// What is mine: the products I sell, then the ones I liked.
@Component({
  selector: 'app-my-products',
  imports: [ProductList, RouterLink],
  templateUrl: './my-products.html',
})
export class MyProducts {}
