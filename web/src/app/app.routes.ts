import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { Home } from './home/home';
import { Catalogue } from './products/catalogue/catalogue';
import { ProductDetail } from './products/product-detail/product-detail';
import { ProductForm } from './products/product-form/product-form';
import { MyProducts } from './products/my-products/my-products';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'products', component: Catalogue },
  { path: 'products/new', component: ProductForm, canActivate: [authGuard] },
  { path: 'products/:id', component: ProductDetail },
  { path: 'products/:id/edit', component: ProductForm, canActivate: [authGuard] },
  { path: 'my-products', component: MyProducts, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
