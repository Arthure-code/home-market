import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Category, Product, ProductDraft, ProductFilter, UploadedPhoto } from './product';

export const PRODUCTS_URL = 'http://localhost:5130/api/products';

// No account name travels with these calls: the API reads it from the
// token the interceptor attaches, when there is one.
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);

  list(filter: ProductFilter = {}): Observable<Product[]> {
    let params = new HttpParams();
    if (filter.q) params = params.set('q', filter.q);
    if (filter.category) params = params.set('category', filter.category);
    return this.http.get<Product[]>(PRODUCTS_URL, { params });
  }

  categories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${PRODUCTS_URL}/categories`);
  }

  get(id: number): Observable<Product> {
    return this.http.get<Product>(`${PRODUCTS_URL}/${id}`);
  }

  mine(): Observable<Product[]> {
    return this.http.get<Product[]>(`${PRODUCTS_URL}/mine`);
  }

  liked(): Observable<Product[]> {
    return this.http.get<Product[]>(`${PRODUCTS_URL}/liked`);
  }

  create(draft: ProductDraft): Observable<Product> {
    return this.http.post<Product>(PRODUCTS_URL, draft);
  }

  update(id: number, draft: ProductDraft): Observable<Product> {
    return this.http.put<Product>(`${PRODUCTS_URL}/${id}`, draft);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${PRODUCTS_URL}/${id}`);
  }

  setLike(id: number, liked: boolean): Observable<void> {
    const url = `${PRODUCTS_URL}/${id}/like`;
    return liked ? this.http.put<void>(url, null) : this.http.delete<void>(url);
  }

  uploadPhoto(file: File): Observable<UploadedPhoto> {
    const form = new FormData();
    form.append('photo', file);
    return this.http.post<UploadedPhoto>(`${PRODUCTS_URL}/photos`, form);
  }
}
