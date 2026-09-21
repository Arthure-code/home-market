import { StockLabelPipe } from './stock-label-pipe';

describe('StockLabelPipe', () => {
  const pipe = new StockLabelPipe();

  it('says in stock, only a few left, or out of stock', () => {
    expect(pipe.transform(12)).toBe('In stock');
    expect(pipe.transform(3)).toBe('Only 3 left');
    expect(pipe.transform(1)).toBe('Only 1 left');
    expect(pipe.transform(0)).toBe('Out of stock');
  });

  it('spells out the long form for a page', () => {
    expect(pipe.transform(12, 'long')).toBe('In stock');
    expect(pipe.transform(2, 'long')).toBe('Only 2 left in stock');
    expect(pipe.transform(0, 'long')).toBe('Out of stock');
  });
});
