import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { ownerGuard } from './guards/owner.guard';
import { SignIn } from './sign-in/sign-in';
import { SignUp } from './sign-up/sign-up';
import { Home } from './home/home';
import { Catalogue } from './products/catalogue/catalogue';
import { ProductDetail } from './products/product-detail/product-detail';
import { ProductNew } from './products/product-new/product-new';
import { ProductEdit } from './products/product-edit/product-edit';
import { MyProducts } from './products/my-products/my-products';
import { MessageList } from './messages/message-list/message-list';
import { MessageView } from './messages/message-view/message-view';
import { Compose } from './messages/compose/compose';
import { CartPage } from './cart/cart-page/cart-page';
import { Checkout } from './orders/checkout/checkout';
import { OrderList } from './orders/order-list/order-list';
import { OrderDetail } from './orders/order-detail/order-detail';
import { Sales } from './orders/sales/sales';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'sign-in', component: SignIn },
  { path: 'sign-up', component: SignUp },
  { path: 'products', component: Catalogue },
  { path: 'products/new', component: ProductNew, canActivate: [authGuard] },
  { path: 'products/:id', component: ProductDetail },
  { path: 'products/:id/edit', component: ProductEdit, canActivate: [authGuard, ownerGuard] },
  { path: 'my-products', component: MyProducts, canActivate: [authGuard] },
  { path: 'messages', component: MessageList, canActivate: [authGuard] },
  { path: 'messages/new', component: Compose, canActivate: [authGuard] },
  { path: 'messages/new/:to', component: Compose, canActivate: [authGuard] },
  { path: 'messages/:id', component: MessageView, canActivate: [authGuard] },
  { path: 'cart', component: CartPage, canActivate: [authGuard] },
  { path: 'checkout', component: Checkout, canActivate: [authGuard] },
  { path: 'orders', component: OrderList, canActivate: [authGuard] },
  { path: 'orders/:id', component: OrderDetail, canActivate: [authGuard] },
  { path: 'sales', component: Sales, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
