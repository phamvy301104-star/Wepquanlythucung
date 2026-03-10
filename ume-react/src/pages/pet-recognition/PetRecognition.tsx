import { useState, useRef, useCallback } from 'react';
import { Link } from 'react-router-dom';
import api, { getImageUrl } from '../../services/api';
import { getBreedInfo, type BreedInfo } from '../../data/breed-info';
import './PetRecognition.scss';

interface EnrichedDetection {
  id: number; type: string; type_vi: string; emoji: string;
  breed: string; breed_vi: string; confidence: number; breed_confidence: number;
  bbox: any; breedInfo: BreedInfo | null; showDetail: boolean;
}

const animalColors: Record<string, string> = {
  dog: '#4CAF50', cat: '#2196F3', bird: '#FF9800', horse: '#9C27B0',
  sheep: '#795548', cow: '#607D8B', elephant: '#455A64', bear: '#E91E63',
  zebra: '#00BCD4', giraffe: '#FF5722',
};

const getConfidenceColor = (conf: number) => conf >= 0.8 ? '#4CAF50' : conf >= 0.5 ? '#FF9800' : '#f44336';

export default function PetRecognition() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [, setSelectedFile] = useState<File | null>(null);
  const [, setImagePreview] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [detections, setDetections] = useState<EnrichedDetection[]>([]);
  const [relatedPetsMap, setRelatedPetsMap] = useState<Record<string, any[]>>({});
  const [relatedProductsMap, setRelatedProductsMap] = useState<Record<string, any[]>>({});

  const processFile = useCallback((file: File) => {
    if (file.size > 10 * 1024 * 1024) { setError('Ảnh quá lớn, vui lòng chọn ảnh dưới 10MB'); return; }
    setSelectedFile(file);
    setError(null); setResult(null); setRelatedPetsMap({}); setRelatedProductsMap({});
    const reader = new FileReader();
    reader.onload = ev => { setImagePreview(ev.target?.result as string); analyzeFile(file, ev.target?.result as string); };
    reader.readAsDataURL(file);
  }, []);

  const analyzeFile = async (file: File, preview: string) => {
    setAnalyzing(true);
    try {
      const fd = new FormData();
      fd.append('file', file);
      const res = await api.post('/ai/detect-pets', fd);
      if (res.data?.success) {
        const data = res.data.data;
        setResult(data);
        const enriched: EnrichedDetection[] = (data.detections || []).map((d: any) => ({
          id: d.id, type: d.type, type_vi: d.type_vi,
          emoji: d.emoji || (d.type === 'dog' ? '🐕' : d.type === 'cat' ? '🐈' : '🐾'),
          breed: d.breed, breed_vi: d.breed_vi,
          confidence: d.confidence, breed_confidence: d.breed_confidence, bbox: d.bbox,
          breedInfo: getBreedInfo(d.breed), showDetail: false,
        }));
        setDetections(enriched);
        setTimeout(() => drawBoundingBoxes(enriched, preview), 150);
        loadRelatedPets(data.detections || []);
        loadRelatedProducts(data.detections || []);
      } else { setError(res.data?.message || 'Không thể phân tích ảnh'); }
    } catch (err: any) { setError(err.response?.data?.message || 'Lỗi kết nối đến dịch vụ AI. Vui lòng thử lại.'); }
    finally { setAnalyzing(false); }
  };

  const loadRelatedPets = async (dets: any[]) => {
    const breeds = [...new Set(dets.map((d: any) => d.breed))] as string[];
    for (const breed of breeds) {
      try {
        const r = await api.get('/pets', { params: { search: breed, limit: 6 } });
        if (r.data?.success && r.data?.data?.pets?.length) {
          setRelatedPetsMap(m => ({ ...m, [breed]: r.data.data.pets }));
        }
      } catch {}
    }
  };

  const loadRelatedProducts = async (dets: any[]) => {
    const types = [...new Set(dets.map((d: any) => d.type))] as string[];
    const searchMap: Record<string, string[]> = {
      dog: ['chó', 'cún'],
      cat: ['mèo'],
    };
    for (const type of types) {
      const keywords = searchMap[type];
      if (!keywords) continue;
      try {
        for (const keyword of keywords) {
          const r = await api.get('/products', { params: { search: keyword, limit: 8 } });
          if (r.data?.success && r.data?.data?.products?.length) {
            setRelatedProductsMap(m => {
              const existing = m[type] || [];
              const existingIds = new Set(existing.map((p: any) => p._id));
              const newProducts = r.data.data.products.filter((p: any) => !existingIds.has(p._id));
              return { ...m, [type]: [...existing, ...newProducts].slice(0, 8) };
            });
          }
        }
      } catch {}
    }
  };

  const drawBoundingBoxes = (dets: EnrichedDetection[], preview: string) => {
    const canvas = canvasRef.current;
    if (!canvas || !preview) return;
    const ctx = canvas.getContext('2d')!;
    const img = new Image();
    img.onload = () => {
      const maxW = Math.min(800, window.innerWidth - 40);
      const scale = maxW / img.width;
      canvas.width = maxW; canvas.height = img.height * scale;
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      for (const det of dets) {
        const b = det.bbox;
        const x = b.x1 * scale, y = b.y1 * scale, w = (b.x2 - b.x1) * scale, h = (b.y2 - b.y1) * scale;
        const color = animalColors[det.type] || '#666';
        ctx.strokeStyle = color; ctx.lineWidth = 3; ctx.strokeRect(x, y, w, h);
        const label = `${det.emoji} ${det.type_vi} #${det.id} - ${det.breed_vi}`;
        ctx.font = 'bold 13px Inter, Arial, sans-serif';
        const tw = ctx.measureText(label).width;
        ctx.fillStyle = color; ctx.globalAlpha = 0.85; ctx.fillRect(x, y - 26, tw + 14, 26); ctx.globalAlpha = 1;
        ctx.fillStyle = '#fff'; ctx.fillText(label, x + 7, y - 8);
        const cl = 15; ctx.strokeStyle = color; ctx.lineWidth = 4;
        ctx.beginPath(); ctx.moveTo(x, y + cl); ctx.lineTo(x, y); ctx.lineTo(x + cl, y); ctx.stroke();
        ctx.beginPath(); ctx.moveTo(x + w - cl, y); ctx.lineTo(x + w, y); ctx.lineTo(x + w, y + cl); ctx.stroke();
        ctx.beginPath(); ctx.moveTo(x, y + h - cl); ctx.lineTo(x, y + h); ctx.lineTo(x + cl, y + h); ctx.stroke();
        ctx.beginPath(); ctx.moveTo(x + w - cl, y + h); ctx.lineTo(x + w, y + h); ctx.lineTo(x + w, y + h - cl); ctx.stroke();
      }
    };
    img.src = preview;
  };

  const toggleDetail = (idx: number) => setDetections(ds => ds.map((d, i) => i === idx ? { ...d, showDetail: !d.showDetail } : d));
  const reset = () => { setResult(null); setDetections([]); setSelectedFile(null); setImagePreview(null); setError(null); setAnalyzing(false); setRelatedPetsMap({}); setRelatedProductsMap({}); };

  return (
    <div className="pet-recognition-page">
      <section className="recognition-hero">
        <div className="container">
          <div className="hero-badge">🤖 Công nghệ YOLO26 & AI</div>
          <h1>✨ Nhận diện động vật bằng AI</h1>
          <p>Tải lên hình ảnh để AI phát hiện và nhận diện: chó, mèo, chim, ngựa, cừu, bò, voi, gấu, ngựa vằn, hươu cao cổ</p>
        </div>
      </section>

      <section className="recognition-content">
        <div className="container">
          {/* Upload area */}
          {!analyzing && !result && (
            <div className="upload-section">
              <div className={`upload-area ${isDragOver ? 'drag-over' : ''}`}
                onDragOver={e => { e.preventDefault(); setIsDragOver(true); }}
                onDragLeave={e => { e.preventDefault(); setIsDragOver(false); }}
                onDrop={e => { e.preventDefault(); setIsDragOver(false); const f = e.dataTransfer?.files[0]; if (f?.type.startsWith('image/')) processFile(f); }}
                onClick={() => document.getElementById('ai-file-input')?.click()}>
                <div className="upload-icon">☁️</div>
                <h3>Kéo thả hoặc nhấp để tải ảnh lên</h3>
                <p>Hỗ trợ: JPG, PNG, WebP — Tối đa 10MB</p>
                <div className="upload-hint">💡 Ảnh rõ nét, chứa động vật sẽ cho kết quả tốt nhất</div>
                <input id="ai-file-input" type="file" accept="image/*" onChange={e => { const f = e.target.files?.[0]; if (f) processFile(f); }} hidden />
              </div>
              <div className="features-row">
                <div className="feature-card"><div className="feat-icon">🔍</div><h4>Phát hiện</h4><p>Đếm số lượng động vật trong ảnh</p></div>
                <div className="feature-card"><div className="feat-icon">🏷️</div><h4>Nhận diện giống</h4><p>Xác định giống từ 120+ loại chó/mèo + 10 loài vật</p></div>
                <div className="feature-card"><div className="feat-icon">📐</div><h4>Bounding Box</h4><p>Khoanh vùng vị trí chính xác</p></div>
              </div>
            </div>
          )}

          {error && <div className="error-alert">⚠️ {error} <button onClick={() => setError(null)}>×</button></div>}

          {analyzing && (
            <div className="analyzing-section">
              <div className="analyzing-animation"><div className="pulse-ring" /><div className="pulse-ring delay" /><div className="ai-icon">🤖</div></div>
              <h3>AI đang phân tích ảnh...</h3><p>Mô hình YOLO đang xử lý, vui lòng chờ</p>
              <div className="loading-dots"><span /><span /><span /></div>
            </div>
          )}

          {result && (
            <div className="results-section">
              <div className="summary-row">
                <div className="summary-card total"><div className="sc-icon">👁️</div><div className="sc-value">{result.total_animals}</div><div className="sc-label">Tổng phát hiện</div></div>
                <div className="summary-card dogs"><div className="sc-icon">🐕</div><div className="sc-value">{result.dogs}</div><div className="sc-label">Chó</div></div>
                <div className="summary-card cats"><div className="sc-icon">🐈</div><div className="sc-value">{result.cats}</div><div className="sc-label">Mèo</div></div>
                {result.other_animals > 0 && <div className="summary-card other"><div className="sc-icon">🐾</div><div className="sc-value">{result.other_animals}</div><div className="sc-label">Động vật khác</div></div>}
              </div>

              {result.total_animals === 0 && <div className="no-detection">😔<h4>Không phát hiện thú cưng</h4><p>Hãy thử tải lên ảnh khác có chứa động vật rõ ràng hơn</p></div>}

              {result.total_animals > 0 && (
                <>
                  <div className="detection-image-section">
                    <h3 className="section-title">📐 Kết quả phát hiện</h3>
                    <div className="canvas-container"><canvas ref={canvasRef} /></div>
                  </div>

                  <div className="detections-section">
                    <h3 className="section-title">📋 Chi tiết nhận diện</h3>
                    <div className="detection-cards">
                      {detections.map((det, idx) => (
                        <div key={det.id} className="det-card" style={{ '--animal-color': animalColors[det.type] || '#666' } as any}>
                          <div className="det-header" style={{ background: `linear-gradient(135deg, ${animalColors[det.type] || '#666'}, ${animalColors[det.type] || '#666'}cc)` }}>
                            <span className="det-emoji">{det.emoji}</span><span className="det-type">{det.type_vi}</span><span className="det-id">#{det.id}</span>
                          </div>
                          <div className="det-body">
                            <h4 className="breed-name">{det.breed_vi}</h4>
                            <p className="breed-en">{det.breed}</p>

                            <div className="confidence-item">
                              <div className="conf-header"><span>Độ tin cậy phát hiện</span><span className="conf-value" style={{ color: getConfidenceColor(det.confidence) }}>{(det.confidence * 100).toFixed(0)}%</span></div>
                              <div className="conf-bar"><div className="conf-fill" style={{ width: `${det.confidence * 100}%`, background: getConfidenceColor(det.confidence) }} /></div>
                            </div>
                            <div className="confidence-item">
                              <div className="conf-header"><span>Độ chính xác giống</span><span className="conf-value" style={{ color: getConfidenceColor(det.breed_confidence) }}>{(det.breed_confidence * 100).toFixed(0)}%</span></div>
                              <div className="conf-bar"><div className="conf-fill" style={{ width: `${det.breed_confidence * 100}%`, background: getConfidenceColor(det.breed_confidence) }} /></div>
                            </div>

                            <button className="btn-toggle-detail" onClick={() => toggleDetail(idx)}>
                              {det.showDetail ? '⬆️ Ẩn thông tin' : '⬇️ Xem chi tiết giống'}
                            </button>

                            {det.showDetail && (
                              <>
                                {det.breedInfo ? (
                                  <div className="inline-breed-detail">
                                    <div className="breed-detail-divider">ℹ️ Thông tin chi tiết giống</div>
                                    <div className="breed-desc">{det.breedInfo.description}</div>
                                    <div className="breed-stats">
                                      <div className="stat-row"><span className="sl">📍 Xuất xứ</span><span className="sv">{det.breedInfo.origin}</span></div>
                                      <div className="stat-row"><span className="sl">📏 Kích thước</span><span className="sv">{det.breedInfo.size}</span></div>
                                      <div className="stat-row"><span className="sl">⚖️ Cân nặng</span><span className="sv">{det.breedInfo.weight}</span></div>
                                      <div className="stat-row"><span className="sl">❤️ Tuổi thọ</span><span className="sv">{det.breedInfo.lifespan}</span></div>
                                      <div className="stat-row"><span className="sl">😊 Tính cách</span><span className="sv">{det.breedInfo.temperament}</span></div>
                                      <div className="stat-row"><span className="sl">🛡️ Chăm sóc</span><span className="sv">{det.breedInfo.careLevel}</span></div>
                                    </div>
                                    <div className="breed-care">
                                      <div className="care-title">📋 Hướng dẫn chăm sóc</div>
                                      <div className="care-row"><span className="care-label">⚡ Vận động:</span> {det.breedInfo.exercise}</div>
                                      <div className="care-row"><span className="care-label">✂️ Chải lông:</span> {det.breedInfo.grooming}</div>
                                      <div className="care-row"><span className="care-label">🏥 Sức khỏe:</span> {det.breedInfo.health}</div>
                                    </div>
                                    <div className="breed-funfact">💡 <strong>Bạn có biết?</strong> {det.breedInfo.funFact}</div>
                                  </div>
                                ) : (
                                  <div className="inline-breed-detail"><div className="breed-desc" style={{ color: '#999' }}>Hiện chưa có dữ liệu chi tiết cho giống {det.breed_vi} ({det.breed}). Chúng tôi đang cập nhật thêm thông tin.</div></div>
                                )}

                                {(relatedPetsMap[det.breed] || []).length > 0 && (
                                  <div className="related-pets-section">
                                    <div className="related-title">🐾 Thú cưng {det.breed_vi} tại cửa hàng</div>
                                    <div className="related-pets-grid">
                                      {relatedPetsMap[det.breed].map(pet => (
                                        <Link key={pet._id} className="related-pet-card" to={`/pets/${pet._id}`}>
                                          <div className="rp-img"><img src={getImageUrl(pet.images?.[0])} alt={pet.name} loading="lazy" /></div>
                                          <div className="rp-info"><div className="rp-name">{pet.name}</div><div className="rp-price">{new Intl.NumberFormat('vi-VN').format(pet.price)}₫</div></div>
                                        </Link>
                                      ))}
                                    </div>
                                  </div>
                                )}

                                {(relatedProductsMap[det.type] || []).length > 0 && (
                                  <div className="related-products-section">
                                    <div className="related-title">🛒 Sản phẩm dành cho {det.type_vi}</div>
                                    <div className="related-products-grid">
                                      {relatedProductsMap[det.type].map(product => (
                                        <Link key={product._id} className="related-product-card" to={`/products/${product.slug || product._id}`}>
                                          <div className="rpr-img"><img src={getImageUrl(product.imageUrl)} alt={product.name} loading="lazy" /></div>
                                          <div className="rpr-info">
                                            <div className="rpr-name">{product.name}</div>
                                            <div className="rpr-price-row">
                                              <span className="rpr-price">{new Intl.NumberFormat('vi-VN').format(product.price)}₫</span>
                                              {product.originalPrice > product.price && (
                                                <span className="rpr-original">{new Intl.NumberFormat('vi-VN').format(product.originalPrice)}₫</span>
                                              )}
                                            </div>
                                            {product.discountPercent > 0 && <span className="rpr-discount">-{product.discountPercent}%</span>}
                                          </div>
                                        </Link>
                                      ))}
                                    </div>
                                  </div>
                                )}
                              </>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </>
              )}

              <div className="action-row"><button className="btn-try-again" onClick={reset}>🔄 Phân tích ảnh khác</button></div>
            </div>
          )}
        </div>
      </section>
    </div>
  );
}
