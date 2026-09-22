// A product as the API describes it, with what it is to the viewer.
export interface Product {
  id: number;
  title: string;
  brand: string;
  maker: string;
  description: string;
  price: number;
  category: string;
  stock: number;
  photoUrl: string;
  seller: string;
  createdAt: string;
  updatedAt: string;
  likes: number;
  liked: boolean;
  mine: boolean;
}

// What a seller sends. The photo is the name the upload route returned.
export interface ProductDraft {
  title: string;
  brand: string;
  maker: string;
  description: string;
  price: number;
  category: string;
  stock: number;
  photo: string;
}

// A category of the shop and how many products it holds.
export interface Category {
  name: string;
  count: number;
}

// How the catalogue can be narrowed: a search text, a category, both.
export interface ProductFilter {
  q?: string;
  category?: string;
}

export interface UploadedPhoto {
  photo: string;
  url: string;
}
