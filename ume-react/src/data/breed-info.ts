/**
 * ====================================================================
 * Thông tin chi tiết giống loài thú cưng (Chó & Mèo)
 * ====================================================================
 * Dùng để hiển thị sau khi AI nhận diện xong.
 * Key = tên giống tiếng Anh (khớp với breed trả về từ AI).
 * ====================================================================
 */

export interface BreedInfo {
  name: string;           // Tên tiếng Anh
  nameVi: string;         // Tên tiếng Việt
  type: 'dog' | 'cat' | 'bird' | 'horse' | 'sheep' | 'cow' | 'elephant' | 'bear' | 'zebra' | 'giraffe';  // Loại
  origin: string;         // Xuất xứ
  size: string;           // Kích thước
  weight: string;         // Cân nặng
  lifespan: string;       // Tuổi thọ
  temperament: string;    // Tính cách
  description: string;    // Mô tả ngắn
  careLevel: string;      // Mức độ chăm sóc
  exercise: string;       // Nhu cầu vận động
  grooming: string;       // Chải lông
  health: string;         // Sức khỏe
  funFact: string;        // Thông tin thú vị
}

export const BREED_DATABASE: Record<string, BreedInfo> = {
  // ========================== CHÓ ==========================

  'Chihuahua': {
    name: 'Chihuahua', nameVi: 'Chihuahua', type: 'dog',
    origin: 'Mexico', size: 'Rất nhỏ (15-23 cm)', weight: '1.5-3 kg', lifespan: '12-20 năm',
    temperament: 'Trung thành, lanh lợi, tự tin, can đảm',
    description: 'Chihuahua là giống chó nhỏ nhất thế giới, có nguồn gốc từ Mexico. Dù nhỏ bé nhưng rất dũng cảm và trung thành.',
    careLevel: 'Thấp', exercise: 'Thấp (30 phút/ngày)', grooming: 'Thấp',
    health: 'Dễ bị vấn đề răng miệng, trật khớp đầu gối', funFact: 'Là giống chó nhỏ nhất thế giới, có thể sống trên 20 năm.'
  },
  'Japanese Spaniel': {
    name: 'Japanese Spaniel', nameVi: 'Nhật Bản Chin', type: 'dog',
    origin: 'Nhật Bản', size: 'Nhỏ (20-27 cm)', weight: '2-5 kg', lifespan: '12-14 năm',
    temperament: 'Thông minh, nhạy cảm, trung thành, ưa yên tĩnh',
    description: 'Chin Nhật Bản là giống chó quý tộc, từng được nuôi trong hoàng cung Nhật Bản.',
    careLevel: 'Trung bình', exercise: 'Thấp (20-30 phút/ngày)', grooming: 'Trung bình',
    health: 'Dễ bị vấn đề hô hấp do mặt ngắn', funFact: 'Thường được coi như mèo vì tính cách thanh lịch và sạch sẽ.'
  },
  'Maltese': {
    name: 'Maltese', nameVi: 'Maltese', type: 'dog',
    origin: 'Malta (Địa Trung Hải)', size: 'Nhỏ (20-25 cm)', weight: '2-4 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, dịu dàng, thích chơi đùa, bám chủ',
    description: 'Maltese có bộ lông trắng mượt như lụa, là một trong những giống chó lâu đời nhất thế giới.',
    careLevel: 'Cao', exercise: 'Thấp-Trung bình', grooming: 'Cao (cần chải lông hàng ngày)',
    health: 'Dễ bị chảy nước mắt, vấn đề răng miệng', funFact: 'Đã được nuôi làm thú cưng hơn 2.000 năm.'
  },
  'Pekingese': {
    name: 'Pekingese', nameVi: 'Bắc Kinh', type: 'dog',
    origin: 'Trung Quốc', size: 'Nhỏ (15-23 cm)', weight: '3-6 kg', lifespan: '12-15 năm',
    temperament: 'Kiêu hãnh, bướng bỉnh, trung thành, dũng cảm',
    description: 'Chó Bắc Kinh từng là giống chó hoàng tộc Trung Quốc, chỉ giới hoàng gia mới được phép nuôi.',
    careLevel: 'Trung bình', exercise: 'Thấp', grooming: 'Cao',
    health: 'Dễ bị vấn đề hô hấp, mắt lồi', funFact: 'Truyền thuyết kể rằng Pekingese là con của sư tử và voọc!'
  },
  'Shih Tzu': {
    name: 'Shih Tzu', nameVi: 'Shih Tzu', type: 'dog',
    origin: 'Tây Tạng / Trung Quốc', size: 'Nhỏ (20-28 cm)', weight: '4-8 kg', lifespan: '10-16 năm',
    temperament: 'Thân thiện, vui vẻ, yêu thương, dễ gần',
    description: 'Shih Tzu nghĩa là "chó sư tử", là giống chó cung đình lâu đời với bộ lông dài mượt đặc trưng.',
    careLevel: 'Cao', exercise: 'Thấp (20-30 phút/ngày)', grooming: 'Cao (chải lông hàng ngày)',
    health: 'Dễ bị vấn đề mắt, hô hấp', funFact: 'Tên "Shih Tzu" trong tiếng Hoa nghĩa là "chó sư tử nhỏ".'
  },
  'Golden Retriever': {
    name: 'Golden Retriever', nameVi: 'Golden Retriever', type: 'dog',
    origin: 'Scotland (Vương quốc Anh)', size: 'Lớn (51-61 cm)', weight: '25-36 kg', lifespan: '10-12 năm',
    temperament: 'Thân thiện, thông minh, trung thành, kiên nhẫn, yêu trẻ em',
    description: 'Golden Retriever là một trong những giống chó được yêu thích nhất thế giới, nổi tiếng với tính cách hiền lành và thông minh.',
    careLevel: 'Trung bình', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Có nguy cơ ung thư, loạn sản khớp háng', funFact: 'Xếp hạng #3 thông minh nhất và là giống chó trị liệu phổ biến nhất.'
  },
  'Labrador Retriever': {
    name: 'Labrador Retriever', nameVi: 'Labrador Retriever', type: 'dog',
    origin: 'Canada (Newfoundland)', size: 'Lớn (54-62 cm)', weight: '25-36 kg', lifespan: '10-12 năm',
    temperament: 'Hiền lành, năng động, thân thiện, trung thành',
    description: 'Labrador là giống chó phổ biến nhất thế giới trong nhiều năm liên tiếp, rất thích nước và bơi lội.',
    careLevel: 'Trung bình', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Trung bình',
    health: 'Dễ béo phì, loạn sản khớp háng', funFact: 'Là giống chó phổ biến #1 tại Mỹ suốt 31 năm liên tiếp.'
  },
  'German Shepherd': {
    name: 'German Shepherd', nameVi: 'Chó chăn cừu Đức', type: 'dog',
    origin: 'Đức', size: 'Lớn (55-65 cm)', weight: '22-40 kg', lifespan: '9-13 năm',
    temperament: 'Thông minh, trung thành, dũng cảm, tự tin',
    description: 'German Shepherd là giống chó đa năng, xuất sắc trong vai trò cảnh sát, quân đội, cứu hộ và chăn gia súc.',
    careLevel: 'Trung bình-Cao', exercise: 'Cao (1.5-2 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Loạn sản khớp háng, vấn đề tiêu hóa', funFact: 'Xếp hạng #3 thông minh nhất, được sử dụng nhiều trong lực lượng cảnh sát.'
  },
  'Poodle': {
    name: 'Poodle', nameVi: 'Poodle', type: 'dog',
    origin: 'Đức / Pháp', size: 'Trung bình-Lớn (35-60 cm)', weight: '3-32 kg', lifespan: '12-15 năm',
    temperament: 'Cực kỳ thông minh, nhạy bén, vui vẻ, năng động',
    description: 'Poodle xếp hạng #2 thông minh nhất thế giới, có 3 kích thước: Toy, Mini và Standard.',
    careLevel: 'Cao', exercise: 'Trung bình-Cao', grooming: 'Cao (không rụng lông nhưng cần cắt tỉa)',
    health: 'Vấn đề mắt, loạn sản khớp háng', funFact: 'Dù trông "điệu đà" nhưng ban đầu là giống chó săn vịt dưới nước.'
  },
  'Toy Poodle': {
    name: 'Toy Poodle', nameVi: 'Poodle Toy', type: 'dog',
    origin: 'Đức / Pháp', size: 'Nhỏ (24-28 cm)', weight: '2-4 kg', lifespan: '14-18 năm',
    temperament: 'Thông minh, vui vẻ, gần gũi, thích được chú ý',
    description: 'Toy Poodle là phiên bản nhỏ nhất của Poodle, rất phổ biến tại Việt Nam vì kích thước nhỏ gọn và không rụng lông.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (30-60 phút/ngày)', grooming: 'Cao',
    health: 'Trật khớp đầu gối, vấn đề răng miệng', funFact: 'Là một trong những giống chó phổ biến nhất tại Việt Nam.'
  },
  'Miniature Poodle': {
    name: 'Miniature Poodle', nameVi: 'Poodle Mini', type: 'dog',
    origin: 'Đức / Pháp', size: 'Nhỏ-Trung bình (28-38 cm)', weight: '5-9 kg', lifespan: '14-16 năm',
    temperament: 'Thông minh, năng động, dễ huấn luyện',
    description: 'Miniature Poodle kết hợp hoàn hảo giữa sự thông minh của Poodle và kích thước vừa phải.',
    careLevel: 'Trung bình', exercise: 'Trung bình (45-60 phút/ngày)', grooming: 'Cao',
    health: 'Vấn đề mắt, khớp háng', funFact: 'Rất giỏi trong các trò xiếc và biểu diễn.'
  },
  'Standard Poodle': {
    name: 'Standard Poodle', nameVi: 'Poodle Tiêu chuẩn', type: 'dog',
    origin: 'Đức / Pháp', size: 'Lớn (45-62 cm)', weight: '20-32 kg', lifespan: '12-15 năm',
    temperament: 'Thong minh, trang nhã, thể thao, trung thành',
    description: 'Standard Poodle là bản gốc lớn nhất, từng được dùng săn vịt nước, rất thể thao và ưa vận động.',
    careLevel: 'Trung bình-Cao', exercise: 'Cao (1 giờ+/ngày)', grooming: 'Cao',
    health: 'Xoắn dạ dày, bệnh Addison', funFact: 'Tên "Poodle" từ tiếng Đức "Pudel" nghĩa là "vẫy nước".'
  },
  'Bulldog': {
    name: 'Bulldog', nameVi: 'Bulldog Anh', type: 'dog',
    origin: 'Anh Quốc', size: 'Trung bình (31-40 cm)', weight: '18-25 kg', lifespan: '8-10 năm',
    temperament: 'Hiền lành, bướng bỉnh, can đảm, thân thiện',
    description: 'Bulldog Anh với khuôn mặt nhăn nheo đặc trưng, tính cách hiền hòa, là biểu tượng của nước Anh.',
    careLevel: 'Trung bình', exercise: 'Thấp (30 phút/ngày)', grooming: 'Thấp (cần lau nếp nhăn)',
    health: 'Vấn đề hô hấp, da, nóng', funFact: 'Ban đầu được lai tạo để đấu bò, nay là giống chó cực hiền.'
  },
  'French Bulldog': {
    name: 'French Bulldog', nameVi: 'Bulldog Pháp', type: 'dog',
    origin: 'Pháp / Anh Quốc', size: 'Nhỏ (27-35 cm)', weight: '8-14 kg', lifespan: '10-12 năm',
    temperament: 'Vui nhộn, thích chơi, bám chủ, dễ thương',
    description: 'French Bulldog với đôi tai dơi đặc trưng, hiện là giống chó phổ biến nhất tại nhiều quốc gia.',
    careLevel: 'Trung bình', exercise: 'Thấp-Trung bình', grooming: 'Thấp',
    health: 'Vấn đề hô hấp, cột sống, không chịu nóng tốt', funFact: 'Hiện là giống chó phổ biến #1 tại Mỹ (2023).'
  },
  'Beagle': {
    name: 'Beagle', nameVi: 'Beagle', type: 'dog',
    origin: 'Anh Quốc', size: 'Trung bình (33-41 cm)', weight: '9-11 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, tò mò, thân thiện, ưa phiêu lưu',
    description: 'Beagle là giống chó săn nổi tiếng với khứu giác siêu nhạy, tính cách thân thiện và yêu đời.',
    careLevel: 'Trung bình', exercise: 'Cao (1 giờ+/ngày)', grooming: 'Thấp',
    health: 'Béo phì, nhiễm trùng tai', funFact: 'Snoopy - chú chó nổi tiếng nhất phim hoạt hình - là Beagle!'
  },
  'Rottweiler': {
    name: 'Rottweiler', nameVi: 'Rottweiler', type: 'dog',
    origin: 'Đức', size: 'Lớn (56-69 cm)', weight: '36-60 kg', lifespan: '8-10 năm',
    temperament: 'Trung thành, tự tin, can đảm, bình tĩnh',
    description: 'Rottweiler là giống chó bảo vệ hàng đầu, rất trung thành và yêu thương gia đình.',
    careLevel: 'Trung bình-Cao', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Thấp',
    health: 'Loạn sản khớp háng, ung thư xương', funFact: 'Từng được dùng kéo xe thịt ở Đức thời La Mã cổ đại.'
  },
  'Yorkshire Terrier': {
    name: 'Yorkshire Terrier', nameVi: 'Yorkshire Terrier', type: 'dog',
    origin: 'Anh Quốc (Yorkshire)', size: 'Nhỏ (17-20 cm)', weight: '2-3 kg', lifespan: '13-16 năm',
    temperament: 'Can đảm, tự tin, thông minh, sống động',
    description: 'Yorkshire Terrier dù nhỏ bé nhưng rất can đảm, có bộ lông dài như tóc người.',
    careLevel: 'Trung bình-Cao', exercise: 'Thấp-Trung bình', grooming: 'Cao',
    health: 'Trật khớp đầu gối, vấn đề răng', funFact: 'Ban đầu được nuôi để bắt chuột trong các nhà máy.'
  },
  'Boxer': {
    name: 'Boxer', nameVi: 'Boxer', type: 'dog',
    origin: 'Đức', size: 'Lớn (53-63 cm)', weight: '25-32 kg', lifespan: '10-12 năm',
    temperament: 'Vui vẻ, năng động, trung thành, thích trẻ em',
    description: 'Boxer là giống chó năng động và yêu đời, rất tốt với trẻ em, được mệnh danh "Peter Pan" của thế giới loài chó.',
    careLevel: 'Trung bình', exercise: 'Cao (1 giờ+/ngày)', grooming: 'Thấp',
    health: 'Ung thư, vấn đề tim mạch', funFact: 'Được đặt tên vì thói quen đứng bằng 2 chân sau và "đấm" như võ sĩ.'
  },
  'Siberian Husky': {
    name: 'Siberian Husky', nameVi: 'Husky Siberia', type: 'dog',
    origin: 'Nga (Siberia)', size: 'Trung bình-Lớn (50-60 cm)', weight: '16-27 kg', lifespan: '12-14 năm',
    temperament: 'Thân thiện, nghịch ngợm, độc lập, yêu tự do',
    description: 'Husky nổi tiếng với đôi mắt xanh tuyệt đẹp và bộ lông dày, được lai tạo để kéo xe trượt tuyết.',
    careLevel: 'Cao', exercise: 'Rất cao (2 giờ+/ngày)', grooming: 'Cao (rụng lông nhiều)',
    health: 'Vấn đề mắt, loạn sản khớp háng', funFact: 'Có thể chạy 150 km/ngày và chịu được -60°C.'
  },
  'Doberman Pinscher': {
    name: 'Doberman Pinscher', nameVi: 'Doberman', type: 'dog',
    origin: 'Đức', size: 'Lớn (63-72 cm)', weight: '32-45 kg', lifespan: '10-13 năm',
    temperament: 'Thông minh, trung thành, cảnh giác, dũng cảm',
    description: 'Doberman là giống chó bảo vệ thông minh bậc nhất, vừa uy nghi vừa duyên dáng.',
    careLevel: 'Trung bình-Cao', exercise: 'Cao (1.5-2 giờ/ngày)', grooming: 'Thấp',
    health: 'Bệnh tim, hội chứng Wobbler', funFact: 'Được tạo ra bởi Karl Friedrich Louis Dobermann - một người thu thuế.'
  },
  'Corgi': {
    name: 'Corgi', nameVi: 'Corgi', type: 'dog',
    origin: 'Wales (Vương quốc Anh)', size: 'Nhỏ-Trung bình (25-30 cm)', weight: '10-14 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, thông minh, can đảm, trung thành',
    description: 'Corgi với chân ngắn đặc trưng, là giống chó yêu thích của Nữ hoàng Elizabeth II.',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao', grooming: 'Trung bình (rụng lông nhiều)',
    health: 'Vấn đề lưng, béo phì', funFact: 'Nữ hoàng Elizabeth II đã nuôi hơn 30 chú Corgi trong đời.'
  },
  'Pembroke Welsh Corgi': {
    name: 'Pembroke Welsh Corgi', nameVi: 'Corgi Pembroke', type: 'dog',
    origin: 'Wales (Vương quốc Anh)', size: 'Nhỏ-Trung bình (25-30 cm)', weight: '10-14 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, thông minh, năng động, thân thiện',
    description: 'Pembroke Welsh Corgi là phiên bản phổ biến hơn, không có đuôi hoặc đuôi rất ngắn.',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao (1 giờ/ngày)', grooming: 'Trung bình',
    health: 'Thoát vị đĩa đệm, béo phì', funFact: 'Truyền thuyết xứ Wales kể rằng Corgi là "ngựa cưỡi của nàng tiên".'
  },
  'Cardigan Welsh Corgi': {
    name: 'Cardigan Welsh Corgi', nameVi: 'Corgi Cardigan', type: 'dog',
    origin: 'Wales (Vương quốc Anh)', size: 'Nhỏ-Trung bình (27-32 cm)', weight: '11-17 kg', lifespan: '12-15 năm',
    temperament: 'Thông minh, trung thành, cảnh giác, vui vẻ',
    description: 'Cardigan Welsh Corgi có đuôi dài, lớn hơn Pembroke một chút, và là giống cổ hơn.',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao', grooming: 'Trung bình',
    health: 'Vấn đề lưng, loạn sản khớp háng', funFact: 'Là một trong những giống chó lâu đời nhất của Quần đảo Anh (3.000 năm).'
  },
  'Dachshund': {
    name: 'Dachshund', nameVi: 'Dachshund (Chó xúc xích)', type: 'dog',
    origin: 'Đức', size: 'Nhỏ (13-23 cm)', weight: '5-15 kg', lifespan: '12-16 năm',
    temperament: 'Can đảm, bướng bỉnh, vui vẻ, tò mò',
    description: 'Dachshund với thân dài chân ngắn đặc trưng, được nuôi để săn lùng thú nhỏ trong hang.',
    careLevel: 'Thấp-Trung bình', exercise: 'Trung bình (30-60 phút/ngày)', grooming: 'Thấp-Trung bình',
    health: 'Thoát vị đĩa đệm (rất phổ biến), béo phì', funFact: 'Tên trong tiếng Đức nghĩa là "chó lửng" vì dùng để săn lửng.'
  },
  'Samoyed': {
    name: 'Samoyed', nameVi: 'Samoyed', type: 'dog',
    origin: 'Nga (Siberia)', size: 'Trung bình-Lớn (48-60 cm)', weight: '16-30 kg', lifespan: '12-14 năm',
    temperament: 'Thân thiện, vui vẻ, hiền lành, thích giao tiếp',
    description: 'Samoyed nổi tiếng với nụ cười đặc trưng và bộ lông trắng muốt, được mệnh danh "chú chó nụ cười".',
    careLevel: 'Cao', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Rất cao (chải lông hàng ngày)',
    health: 'Loạn sản khớp háng, bệnh tim', funFact: 'Nụ cười "Sammy smile" khiến chúng không bao giờ chảy dãi.'
  },
  'Pomeranian': {
    name: 'Pomeranian', nameVi: 'Phốc sóc (Pomeranian)', type: 'dog',
    origin: 'Đức / Ba Lan', size: 'Nhỏ (18-22 cm)', weight: '1.5-3.5 kg', lifespan: '12-16 năm',
    temperament: 'Sống động, tò mò, can đảm, thông minh',
    description: 'Pomeranian hay Phốc sóc là giống chó cảnh phổ biến với bộ lông bông xù như quả cầu lông.',
    careLevel: 'Trung bình', exercise: 'Thấp-Trung bình (30 phút/ngày)', grooming: 'Cao',
    health: 'Trật khớp đầu gối, sập khí quản', funFact: 'Tổ tiên của chúng từng nặng 14 kg, gấp 5 lần hiện tại!'
  },
  'Border Collie': {
    name: 'Border Collie', nameVi: 'Border Collie', type: 'dog',
    origin: 'Scotland / Anh Quốc', size: 'Trung bình (46-56 cm)', weight: '14-20 kg', lifespan: '12-15 năm',
    temperament: 'Cực kỳ thông minh, năng động, ham học hỏi, trung thành',
    description: 'Border Collie được công nhận là giống chó thông minh nhất thế giới, xuất sắc trong chăn cừu và agility.',
    careLevel: 'Cao', exercise: 'Rất cao (2 giờ+/ngày)', grooming: 'Trung bình',
    health: 'Dị tật mắt Collie, động kinh', funFact: 'Chaser - Border Collie nổi tiếng nhất - có thể nhớ tên hơn 1.000 đồ vật.'
  },
  'Australian Shepherd': {
    name: 'Australian Shepherd', nameVi: 'Chó chăn cừu Úc', type: 'dog',
    origin: 'Mỹ (không phải Úc)', size: 'Trung bình (46-58 cm)', weight: '18-29 kg', lifespan: '12-15 năm',
    temperament: 'Thông minh, năng động, trung thành, thích làm việc',
    description: 'Australian Shepherd thực ra có nguồn gốc từ Mỹ, nổi tiếng với đôi mắt nhiều màu và bộ lông merle.',
    careLevel: 'Cao', exercise: 'Rất cao (1.5-2 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Dị tật mắt, điếc (màu merle)', funFact: 'Dù tên "Úc" nhưng thực chất được phát triển tại miền Tây nước Mỹ.'
  },
  'Akita': {
    name: 'Akita', nameVi: 'Akita', type: 'dog',
    origin: 'Nhật Bản', size: 'Lớn (60-71 cm)', weight: '32-59 kg', lifespan: '10-13 năm',
    temperament: 'Trung thành, can đảm, uy nghiêm, bảo vệ',
    description: 'Akita là giống chó biểu tượng của Nhật Bản, nổi tiếng qua câu chuyện Hachiko.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình-Cao (1 giờ/ngày)', grooming: 'Cao (rụng lông nhiều)',
    health: 'Loạn sản khớp háng, bệnh tự miễn', funFact: 'Hachiko - chú Akita trung thành nhất - đã đợi chủ 9 năm tại ga tàu.'
  },
  'Great Dane': {
    name: 'Great Dane', nameVi: 'Great Dane', type: 'dog',
    origin: 'Đức', size: 'Khổng lồ (71-86 cm)', weight: '45-90 kg', lifespan: '7-10 năm',
    temperament: 'Hiền lành, kiên nhẫn, thân thiện, bình tĩnh',
    description: 'Great Dane là một trong những giống chó lớn nhất thế giới, được mệnh danh "người khổng lồ hiền lành".',
    careLevel: 'Trung bình', exercise: 'Trung bình (1 giờ/ngày)', grooming: 'Thấp',
    health: 'Xoắn dạ dày, bệnh tim, tuổi thọ ngắn', funFact: 'Scooby-Doo là một chú Great Dane! Kỷ lục cao nhất: 112 cm.'
  },
  'Dalmatian': {
    name: 'Dalmatian', nameVi: 'Dalmatian (Đốm)', type: 'dog',
    origin: 'Croatia (Dalmatia)', size: 'Lớn (48-61 cm)', weight: '23-25 kg', lifespan: '10-13 năm',
    temperament: 'Năng động, thông minh, nhạy cảm, chung thủy',
    description: 'Dalmatian nổi tiếng với bộ lông trắng đen đốm độc đáo, trở nên nổi tiếng qua phim "101 Dalmatians".',
    careLevel: 'Trung bình-Cao', exercise: 'Rất cao (2 giờ+/ngày)', grooming: 'Thấp (nhưng rụng lông nhiều)',
    health: 'Điếc bẩm sinh (30%), sỏi thận', funFact: 'Sinh ra hoàn toàn trắng, các đốm xuất hiện sau 2-4 tuần tuổi.'
  },
  'Cocker Spaniel': {
    name: 'Cocker Spaniel', nameVi: 'Cocker Spaniel', type: 'dog',
    origin: 'Anh Quốc / Mỹ', size: 'Trung bình (36-43 cm)', weight: '12-16 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, hiền lành, thân thiện, thích chơi',
    description: 'Cocker Spaniel với đôi tai dài rủ đặc trưng, luôn vui vẻ và tràn đầy năng lượng.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình-Cao', grooming: 'Cao (tai dài cần chăm sóc kỹ)',
    health: 'Nhiễm trùng tai, vấn đề mắt', funFact: 'Lady trong phim "Lady and the Tramp" của Disney là Cocker Spaniel.'
  },
  'Bernese Mountain Dog': {
    name: 'Bernese Mountain Dog', nameVi: 'Chó núi Bernese', type: 'dog',
    origin: 'Thụy Sĩ', size: 'Lớn (58-70 cm)', weight: '32-52 kg', lifespan: '7-10 năm',
    temperament: 'Hiền lành, trung thành, bình tĩnh, yêu trẻ',
    description: 'Bernese Mountain Dog là giống chó lớn với bộ lông 3 màu đen-trắng-nâu đặc trưng, rất tốt với gia đình.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (1 giờ/ngày)', grooming: 'Cao (rụng lông nhiều)',
    health: 'Ung thư, loạn sản khớp háng, tuổi thọ ngắn', funFact: 'Từng được dùng kéo xe sữa ở vùng núi Alps Thụy Sĩ.'
  },
  'Shiba Inu': {
    name: 'Shiba Inu', nameVi: 'Shiba Inu', type: 'dog',
    origin: 'Nhật Bản', size: 'Nhỏ-Trung bình (33-43 cm)', weight: '8-11 kg', lifespan: '12-16 năm',
    temperament: 'Độc lập, tự tin, lanh lợi, trung thành',
    description: 'Shiba Inu là giống chó nhỏ nhất của Nhật Bản, nổi tiếng meme "Doge" và biểu cảm phong phú.',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao (1 giờ/ngày)', grooming: 'Trung bình (rụng lông theo mùa)',
    health: 'Dị ứng, trật khớp đầu gối', funFact: 'Là nguồn cảm hứng của meme "Doge" và tiền mã hóa Dogecoin.'
  },
  'Chow Chow': {
    name: 'Chow Chow', nameVi: 'Chow Chow', type: 'dog',
    origin: 'Trung Quốc', size: 'Trung bình (43-51 cm)', weight: '20-32 kg', lifespan: '9-15 năm',
    temperament: 'Độc lập, trung thành, kiêu hãnh, bảo vệ',
    description: 'Chow Chow nổi tiếng với lưỡi xanh-đen độc đáo và bộ lông bồng bềnh như sư tử.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (45 phút/ngày)', grooming: 'Cao',
    health: 'Vấn đề khớp, mắt, da', funFact: 'Là 1 trong 2 giống chó duy nhất có lưỡi màu xanh-đen (cùng Shar Pei).'
  },
  'Alaskan Malamute': {
    name: 'Alaskan Malamute', nameVi: 'Alaska Malamute', type: 'dog',
    origin: 'Mỹ (Alaska)', size: 'Lớn (58-71 cm)', weight: '34-45 kg', lifespan: '10-14 năm',
    temperament: 'Trung thành, thân thiện, mạnh mẽ, bướng bỉnh',
    description: 'Alaskan Malamute là giống chó kéo xe lớn nhất, mạnh mẽ và bền bỉ, thích hợp với khí hậu lạnh.',
    careLevel: 'Cao', exercise: 'Rất cao (2 giờ+/ngày)', grooming: 'Cao (rụng lông rất nhiều)',
    health: 'Loạn sản khớp háng, bệnh tuyến giáp', funFact: 'Có thể kéo trọng tải lên tới 500 kg và chịu được nhiệt độ -40°C.'
  },
  'Pug': {
    name: 'Pug', nameVi: 'Pug', type: 'dog',
    origin: 'Trung Quốc', size: 'Nhỏ (25-33 cm)', weight: '6-9 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, quyến rũ, hài hước, bám chủ',
    description: 'Pug với khuôn mặt nhăn nheo đáng yêu, là giống chó cảnh được yêu thích với tính cách hài hước.',
    careLevel: 'Thấp-Trung bình', exercise: 'Thấp (30 phút/ngày)', grooming: 'Thấp (cần lau nếp nhăn)',
    health: 'Vấn đề hô hấp, mắt lồi, không chịu nóng', funFact: 'Tên "Pug" nghĩa là "nắm đấm" vì mặt giống nắm đấm.'
  },
  'Schnauzer': {
    name: 'Schnauzer', nameVi: 'Schnauzer', type: 'dog',
    origin: 'Đức', size: 'Trung bình (45-50 cm)', weight: '14-20 kg', lifespan: '13-16 năm',
    temperament: 'Thông minh, cảnh giác, dũng cảm, trung thành',
    description: 'Schnauzer nổi bật với bộ râu và lông mày rậm đặc trưng, là giống chó bảo vệ đa năng.',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao (1 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Viêm tụy, sỏi bàng quang', funFact: 'Tên "Schnauzer" từ tiếng Đức "Schnauze" nghĩa là "mõm".'
  },
  'Miniature Schnauzer': {
    name: 'Miniature Schnauzer', nameVi: 'Schnauzer Mini', type: 'dog',
    origin: 'Đức', size: 'Nhỏ (30-36 cm)', weight: '5-9 kg', lifespan: '12-15 năm',
    temperament: 'Vui vẻ, thông minh, can đảm, thân thiện',
    description: 'Miniature Schnauzer là phiên bản thu nhỏ, rất phổ biến vì kích thước vừa phải và không rụng lông.',
    careLevel: 'Trung bình', exercise: 'Trung bình (45 phút/ngày)', grooming: 'Trung bình-Cao',
    health: 'Viêm tụy, sỏi bàng quang, tiểu đường', funFact: 'Là giống Schnauzer phổ biến nhất và nằm trong top 20 giống chó ưa thích tại Mỹ.'
  },
  'Giant Schnauzer': {
    name: 'Giant Schnauzer', nameVi: 'Schnauzer Khổng lồ', type: 'dog',
    origin: 'Đức', size: 'Lớn (60-70 cm)', weight: '27-48 kg', lifespan: '12-15 năm',
    temperament: 'Mạnh mẽ, kiên quyết, trung thành, cảnh giác',
    description: 'Giant Schnauzer là phiên bản lớn nhất, từng được dùng canh gác và trong lực lượng cảnh sát.',
    careLevel: 'Cao', exercise: 'Cao (1.5 giờ+/ngày)', grooming: 'Cao',
    health: 'Loạn sản khớp háng, ung thư', funFact: 'Từng được huấn luyện làm chó cảnh sát tại Đức và Nga.'
  },
  'Bichon Frise': {
    name: 'Bichon Frise', nameVi: 'Bichon Frise', type: 'dog',
    origin: 'Pháp / Bỉ', size: 'Nhỏ (23-30 cm)', weight: '3-5 kg', lifespan: '14-15 năm',
    temperament: 'Vui vẻ, nhạy cảm, thân thiện, thích biểu diễn',
    description: 'Bichon Frise với bộ lông trắng bông xoăn như mây, luôn tươi vui và thích giao tiếp.',
    careLevel: 'Cao', exercise: 'Trung bình (30-60 phút/ngày)', grooming: 'Rất cao (chải lông hàng ngày)',
    health: 'Dị ứng da, trật khớp đầu gối', funFact: 'Từng là giống chó biểu diễn xiếc rất phổ biến ở châu Âu thời Trung cổ.'
  },
  'Lhasa Apso': {
    name: 'Lhasa Apso', nameVi: 'Lhasa Apso', type: 'dog',
    origin: 'Tây Tạng', size: 'Nhỏ (25-28 cm)', weight: '5-8 kg', lifespan: '12-15 năm',
    temperament: 'Tự tin, cảnh giác, vui vẻ, kiêu hãnh',
    description: 'Lhasa Apso là giống chó canh gác cổ đại của các tu viện Tây Tạng với bộ lông dài chạm đất.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (30-45 phút/ngày)', grooming: 'Cao',
    health: 'Vấn đề thận, mắt', funFact: 'Được coi là linh vật may mắn ở Tây Tạng, tên nghĩa là "chó canh gác có lông".'
  },
  'Shar Pei': {
    name: 'Shar Pei', nameVi: 'Shar Pei', type: 'dog',
    origin: 'Trung Quốc', size: 'Trung bình (44-51 cm)', weight: '18-25 kg', lifespan: '8-12 năm',
    temperament: 'Trung thành, độc lập, bình tĩnh, bảo vệ',
    description: 'Shar Pei nổi tiếng với làn da nhăn nheo và lưỡi xanh-đen, từng suýt tuyệt chủng vào những năm 1970.',
    careLevel: 'Trung bình', exercise: 'Trung bình (45 phút/ngày)', grooming: 'Thấp (lau nếp nhăn)',
    health: 'Vấn đề da, mắt, sốt Shar Pei', funFact: 'Từng được sách Guinness ghi nhận là giống chó hiếm nhất thế giới.'
  },
  'Saint Bernard': {
    name: 'Saint Bernard', nameVi: 'Saint Bernard', type: 'dog',
    origin: 'Thụy Sĩ / Ý', size: 'Khổng lồ (65-90 cm)', weight: '54-120 kg', lifespan: '8-10 năm',
    temperament: 'Hiền lành, kiên nhẫn, thân thiện, bảo vệ',
    description: 'Saint Bernard là giống chó cứu hộ nổi tiếng trên dãy Alps, rất hiền lành dù kích thước khổng lồ.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (1 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Xoắn dạ dày, loạn sản khớp háng', funFact: 'Chú Saint Bernard tên Barry đã cứu hơn 40 người trong bão tuyết.'
  },
  'Basset Hound': {
    name: 'Basset Hound', nameVi: 'Basset Hound', type: 'dog',
    origin: 'Pháp', size: 'Trung bình (28-38 cm)', weight: '20-29 kg', lifespan: '12-13 năm',
    temperament: 'Hiền lành, kiên nhẫn, trung thành, bướng bỉnh',
    description: 'Basset Hound với đôi tai dài rủ và ánh mắt buồn đặc trưng, có khứu giác chỉ thua Bloodhound.',
    careLevel: 'Thấp-Trung bình', exercise: 'Trung bình (30-60 phút/ngày)', grooming: 'Thấp',
    health: 'Vấn đề tai, béo phì, thoát vị đĩa đệm', funFact: 'Là biểu tượng của thương hiệu giày Hush Puppies.'
  },
  'Dandie Dinmont Terrier': {
    name: 'Dandie Dinmont Terrier', nameVi: 'Dandie Dinmont Terrier', type: 'dog',
    origin: 'Scotland', size: 'Nhỏ (20-28 cm)', weight: '8-11 kg', lifespan: '12-15 năm',
    temperament: 'Độc lập, thông minh, dũng cảm, quyết đoán',
    description: 'Dandie Dinmont Terrier có chỏm lông trên đầu đặc trưng, là giống terrier hiếm và cổ xưa.',
    careLevel: 'Trung bình', exercise: 'Trung bình (30-45 phút/ngày)', grooming: 'Trung bình',
    health: 'Vấn đề lưng, glaucoma', funFact: 'Là giống chó duy nhất được đặt tên theo nhân vật tiểu thuyết (của Sir Walter Scott).'
  },
  'Redbone Coonhound': {
    name: 'Redbone Coonhound', nameVi: 'Redbone Coonhound', type: 'dog',
    origin: 'Mỹ', size: 'Lớn (53-68 cm)', weight: '20-32 kg', lifespan: '12-15 năm',
    temperament: 'Thân thiện, năng động, kiên trì, hào phóng',
    description: 'Redbone Coonhound với bộ lông đỏ đặc trưng, là giống chó săn gấu trúc Mỹ, rất giỏi theo dấu.',
    careLevel: 'Trung bình', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Thấp',
    health: 'Loạn sản khớp háng, nhiễm trùng tai', funFact: 'Nổi tiếng qua tiểu thuyết "Where the Red Fern Grows" — câu chuyện cảm động về hai chú Redbone.'
  },
  'Papillon': {
    name: 'Papillon', nameVi: 'Papillon', type: 'dog',
    origin: 'Pháp / Bỉ', size: 'Nhỏ (20-28 cm)', weight: '2-5 kg', lifespan: '14-16 năm',
    temperament: 'Thông minh, vui vẻ, thân thiện, năng động',
    description: 'Papillon với đôi tai giống cánh bướm đặc trưng, là một trong những giống chó nhỏ thông minh nhất.',
    careLevel: 'Trung bình', exercise: 'Trung bình (30-60 phút/ngày)', grooming: 'Trung bình',
    health: 'Trật khớp đầu gối, vấn đề răng', funFact: 'Tên "Papillon" nghĩa là "bướm" trong tiếng Pháp vì hình dáng tai.'
  },
  'Weimaraner': {
    name: 'Weimaraner', nameVi: 'Weimaraner', type: 'dog',
    origin: 'Đức', size: 'Lớn (56-69 cm)', weight: '25-40 kg', lifespan: '10-13 năm',
    temperament: 'Năng động, thông minh, trung thành, can đảm',
    description: 'Weimaraner với bộ lông xám bạc đặc trưng, được mệnh danh "bóng ma xám", là giống chó săn quý tộc.',
    careLevel: 'Cao', exercise: 'Rất cao (2 giờ+/ngày)', grooming: 'Thấp',
    health: 'Xoắn dạ dày, loạn sản khớp háng', funFact: 'Ban đầu chỉ giới quý tộc Weimar (Đức) mới được phép nuôi.'
  },
  'Brittany': {
    name: 'Brittany', nameVi: 'Brittany', type: 'dog',
    origin: 'Pháp (Bretagne)', size: 'Trung bình (44-52 cm)', weight: '14-18 kg', lifespan: '12-14 năm',
    temperament: 'Năng động, vui vẻ, thông minh, thích làm việc',
    description: 'Brittany là giống chó săn đa năng, vừa chỉ điểm vừa tha mồi, rất năng động và dễ huấn luyện.',
    careLevel: 'Trung bình', exercise: 'Rất cao (1.5-2 giờ/ngày)', grooming: 'Trung bình',
    health: 'Loạn sản khớp háng, động kinh', funFact: 'Là giống chó chiến thắng nhiều nhất trong các cuộc thi chó săn tại Mỹ.'
  },
  'Irish Setter': {
    name: 'Irish Setter', nameVi: 'Irish Setter', type: 'dog',
    origin: 'Ireland', size: 'Lớn (55-67 cm)', weight: '27-32 kg', lifespan: '12-15 năm',
    temperament: 'Thân thiện, tinh nghịch, năng động, yêu đời',
    description: 'Irish Setter với bộ lông đỏ mahogany lộng lẫy, là một trong những giống chó đẹp nhất thế giới.',
    careLevel: 'Trung bình-Cao', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Xoắn dạ dày, động kinh', funFact: 'Tổng thống Mỹ Richard Nixon từng nuôi một chú Irish Setter tên King Timahoe.'
  },
  'English Setter': {
    name: 'English Setter', nameVi: 'English Setter', type: 'dog',
    origin: 'Anh Quốc', size: 'Lớn (61-69 cm)', weight: '20-36 kg', lifespan: '12 năm',
    temperament: 'Hiền lành, dịu dàng, thân thiện, kiên nhẫn',
    description: 'English Setter là giống chó săn trang nhã với bộ lông đốm phết đặc trưng, rất hiền lành.',
    careLevel: 'Trung bình', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Trung bình-Cao',
    health: 'Điếc, loạn sản khớp háng', funFact: 'Là một trong những giống chó săn lâu đời nhất, có từ thế kỷ 14.'
  },
  'Cane Corso': {
    name: 'Cane Corso', nameVi: 'Cane Corso', type: 'dog',
    origin: 'Ý', size: 'Lớn (60-70 cm)', weight: '40-50 kg', lifespan: '9-12 năm',
    temperament: 'Uy nghiêm, trung thành, thông minh, bảo vệ',
    description: 'Cane Corso là giống chó bảo vệ Ý cổ đại, hậu duệ của chó chiến La Mã, rất mạnh mẽ và trung thành.',
    careLevel: 'Cao', exercise: 'Cao (1-2 giờ/ngày)', grooming: 'Thấp',
    health: 'Loạn sản khớp háng, xoắn dạ dày', funFact: 'Tên "Corso" từ tiếng Latin "cohors" nghĩa là "người bảo vệ".'
  },
  'Italian Greyhound': {
    name: 'Italian Greyhound', nameVi: 'Greyhound Ý', type: 'dog',
    origin: 'Ý', size: 'Nhỏ (33-38 cm)', weight: '3-5 kg', lifespan: '14-15 năm',
    temperament: 'Nhạy cảm, dịu dàng, vui vẻ, bám chủ',
    description: 'Italian Greyhound là phiên bản thu nhỏ của Greyhound, thanh mảnh và duyên dáng như tác phẩm nghệ thuật.',
    careLevel: 'Thấp-Trung bình', exercise: 'Trung bình (30-45 phút/ngày)', grooming: 'Thấp',
    health: 'Gãy xương (xương mỏng), vấn đề răng', funFact: 'Từng được các pharaoh Ai Cập và hoàng gia châu Âu yêu thích.'
  },

  // ========================== MÈO ==========================

  'Tabby Cat': {
    name: 'Tabby Cat', nameVi: 'Mèo Mướp (Tabby)', type: 'cat',
    origin: 'Toàn cầu', size: 'Trung bình (23-28 cm)', weight: '3-7 kg', lifespan: '12-18 năm',
    temperament: 'Thân thiện, vui vẻ, thích khám phá, gần gũi',
    description: 'Mèo Mướp (Tabby) không phải giống mà là họa tiết lông vằn đặc trưng, rất phổ biến tại Việt Nam.',
    careLevel: 'Thấp', exercise: 'Trung bình', grooming: 'Thấp',
    health: 'Sức khỏe tốt, ít bệnh di truyền', funFact: 'Khoảng 80% mèo trên thế giới có họa tiết tabby. Chữ "M" trên trán là đặc điểm nhận dạng.'
  },
  'Tiger Cat': {
    name: 'Tiger Cat', nameVi: 'Mèo Vằn', type: 'cat',
    origin: 'Toàn cầu', size: 'Trung bình (23-28 cm)', weight: '3-7 kg', lifespan: '12-18 năm',
    temperament: 'Năng động, nhanh nhẹn, thích săn mồi, thông minh',
    description: 'Mèo Vằn có bộ lông sọc tương tự hổ, bản năng săn mồi mạnh, rất linh hoạt.',
    careLevel: 'Thấp', exercise: 'Trung bình-Cao', grooming: 'Thấp',
    health: 'Sức khỏe tốt, thể chất khỏe mạnh', funFact: 'Họa tiết vằn giúp ngụy trang tốt khi săn mồi trong tự nhiên.'
  },
  'Persian Cat': {
    name: 'Persian Cat', nameVi: 'Mèo Ba Tư', type: 'cat',
    origin: 'Iran (Ba Tư)', size: 'Trung bình (25-38 cm)', weight: '3-7 kg', lifespan: '12-17 năm',
    temperament: 'Bình tĩnh, dịu dàng, yên lặng, ưa nằm',
    description: 'Mèo Ba Tư nổi tiếng với mặt phẳng và bộ lông dài mượt sang trọng, là "quý tộc" của thế giới mèo.',
    careLevel: 'Cao', exercise: 'Thấp', grooming: 'Rất cao (chải lông hàng ngày)',
    health: 'Bệnh thận đa nang, vấn đề hô hấp', funFact: 'Mèo Ba Tư là giống mèo lâu đời nhất, có từ 1684 trước Công nguyên.'
  },
  'Siamese Cat': {
    name: 'Siamese Cat', nameVi: 'Mèo Xiêm (Siamese)', type: 'cat',
    origin: 'Thái Lan (Siam)', size: 'Trung bình (29-31 cm)', weight: '3-5 kg', lifespan: '15-20 năm',
    temperament: 'Hoạt bát, thích nói chuyện, thông minh, bám chủ',
    description: 'Mèo Xiêm với đôi mắt xanh sapphire đặc trưng và bộ lông kem với điểm màu sẫm, rất "nói chuyện".',
    careLevel: 'Trung bình', exercise: 'Trung bình-Cao', grooming: 'Thấp',
    health: 'Lé mắt, bệnh amyloidosis', funFact: 'Sinh ra hoàn toàn trắng, các điểm màu sẫm xuất hiện sau vài tuần do enzyme nhạy nhiệt.'
  },
  'Egyptian Mau': {
    name: 'Egyptian Mau', nameVi: 'Mèo Ai Cập (Egyptian Mau)', type: 'cat',
    origin: 'Ai Cập', size: 'Trung bình (25-30 cm)', weight: '3-5 kg', lifespan: '12-15 năm',
    temperament: 'Nhanh nhẹn, trung thành, nhạy cảm, yêu gia đình',
    description: 'Egyptian Mau là giống mèo tự nhiên duy nhất có đốm, và là giống mèo nhà nhanh nhất (48 km/h).',
    careLevel: 'Thấp-Trung bình', exercise: 'Cao (rất năng động)', grooming: 'Thấp',
    health: 'Sức khỏe tốt, ít bệnh di truyền', funFact: 'Là giống mèo nhà nhanh nhất thế giới, đạt 48 km/h. "Mau" nghĩa là "mèo" trong tiếng Ai Cập cổ.'
  },

  // ========================== ĐỘNG VẬT KHÁC (COCO) ==========================

  'bird': {
    name: 'Bird', nameVi: 'Chim', type: 'bird',
    origin: 'Toàn cầu', size: 'Đa dạng (5 cm – 2.7 m)', weight: '2 g – 150 kg', lifespan: '4-80 năm (tùy loài)',
    temperament: 'Thông minh, hoạt bát, hòa đồng, hát hay',
    description: 'Chim là nhóm động vật có xương sống, máu nóng, có lông vũ và mỏ. Trên thế giới có hơn 10.000 loài chim khác nhau.',
    careLevel: 'Trung bình', exercise: 'Trung bình (cần không gian bay)', grooming: 'Thấp',
    health: 'Chú ý nhiệt độ, bệnh đường hô hấp, dinh dưỡng cân bằng', funFact: 'Vẹt xám châu Phi có thể học hơn 1.000 từ và hiểu ngữ cảnh đơn giản.'
  },
  'horse': {
    name: 'Horse', nameVi: 'Ngựa', type: 'horse',
    origin: 'Trung Á (thuần hóa ~4000 TCN)', size: 'Rất lớn (1.4-1.8 m vai)', weight: '380-1.000 kg', lifespan: '25-30 năm',
    temperament: 'Trung thành, thông minh, nhạy cảm, mạnh mẽ',
    description: 'Ngựa là loài động vật có vú được thuần hóa từ hàng nghìn năm. Chúng đóng vai trò lớn trong lịch sử giao thông, nông nghiệp và thể thao.',
    careLevel: 'Cao', exercise: 'Rất cao (cần chạy và vận động mỗi ngày)', grooming: 'Cao (chải lông, chăm sóc móng)',
    health: 'Đau bụng (colic), viêm móng, bệnh đường hô hấp', funFact: 'Ngựa có thể ngủ đứng nhờ cơ chế khóa khớp chân đặc biệt.'
  },
  'sheep': {
    name: 'Sheep', nameVi: 'Cừu', type: 'sheep',
    origin: 'Mesopotamia (~8000 TCN)', size: 'Trung bình-Lớn (60-90 cm vai)', weight: '45-160 kg', lifespan: '10-12 năm',
    temperament: 'Hiền lành, bầy đàn, nhút nhát, dễ nuôi',
    description: 'Cừu là một trong những loài vật nuôi đầu tiên, cung cấp len, thịt và sữa. Chúng sống theo bầy và có trí nhớ tốt.',
    careLevel: 'Trung bình', exercise: 'Trung bình (thích đi lại trên đồng cỏ)', grooming: 'Cao (cắt lông định kỳ)',
    health: 'Ký sinh trùng, bệnh chân móng, viêm phổi', funFact: 'Cừu có thể nhận diện được đến 50 khuôn mặt cừu khác nhau và nhớ trong 2 năm.'
  },
  'cow': {
    name: 'Cow', nameVi: 'Bò', type: 'cow',
    origin: 'Cận Đông (~8000 TCN)', size: 'Rất lớn (1.2-1.5 m vai)', weight: '400-1.000 kg', lifespan: '18-22 năm',
    temperament: 'Hiền lành, tò mò, bầy đàn, thân thiện',
    description: 'Bò là loài gia súc quan trọng nhất thế giới, cung cấp sữa, thịt và sức kéo. Bò có hệ tiêu hóa 4 ngăn dạ dày đặc biệt.',
    careLevel: 'Trung bình-Cao', exercise: 'Trung bình (cần bãi chăn thả)', grooming: 'Thấp',
    health: 'Bệnh lở mồm long móng, viêm vú, ký sinh trùng', funFact: 'Bò có bạn thân và sẽ bị stress khi bị tách khỏi bạn.'
  },
  'elephant': {
    name: 'Elephant', nameVi: 'Voi', type: 'elephant',
    origin: 'Châu Phi & Châu Á', size: 'Khổng lồ (2.5-4 m vai)', weight: '2.700-6.000 kg', lifespan: '60-70 năm',
    temperament: 'Thông minh, tình cảm, bầy đàn, trí nhớ siêu phàm',
    description: 'Voi là loài động vật trên cạn lớn nhất hành tinh. Chúng cực kỳ thông minh, có cảm xúc phức tạp và sống trong cấu trúc xã hội đàn.',
    careLevel: 'Rất cao (bảo tồn)', exercise: 'Rất cao (đi bộ hàng chục km/ngày)', grooming: 'Thấp (tự tắm bùn)',
    health: 'Bệnh lao, herpes virus, vấn đề chân do nuôi nhốt', funFact: 'Voi là loài duy nhất ngoài con người biết thực hiện nghi lễ "tang lễ" cho đồng loại đã mất.'
  },
  'bear': {
    name: 'Bear', nameVi: 'Gấu', type: 'bear',
    origin: 'Bắc Mỹ, Châu Á, Châu Âu', size: 'Rất lớn (1-3 m đứng)', weight: '60-600 kg', lifespan: '20-35 năm',
    temperament: 'Đơn độc, thông minh, tò mò, sức mạnh lớn',
    description: 'Gấu là loài ăn tạp lớn, gồm 8 loài khác nhau. Chúng nổi tiếng với sức mạnh, khứu giác tuyệt vời và khả năng ngủ đông.',
    careLevel: 'Bảo tồn (không nuôi)', exercise: 'Rất cao (phạm vi sống rộng)', grooming: 'Không áp dụng',
    health: 'Bệnh ký sinh trùng, mange, vấn đề răng', funFact: 'Gấu nâu có khứu giác mạnh gấp 2.100 lần con người, có thể đánh hơi thức ăn cách 30 km.'
  },
  'zebra': {
    name: 'Zebra', nameVi: 'Ngựa vằn', type: 'zebra',
    origin: 'Châu Phi', size: 'Lớn (1.2-1.5 m vai)', weight: '350-450 kg', lifespan: '25-30 năm',
    temperament: 'Hoang dã, cảnh giác, bầy đàn, khó thuần hóa',
    description: 'Ngựa vằn là loài ngựa hoang châu Phi nổi bật với bộ lông sọc đen trắng độc đáo. Mỗi con có hoa văn sọc riêng biệt.',
    careLevel: 'Bảo tồn (hoang dã)', exercise: 'Rất cao (di cư hàng trăm km)', grooming: 'Không áp dụng',
    health: 'Bệnh ký sinh trùng, bệnh ngựa châu Phi', funFact: 'Không có hai con ngựa vằn nào có hoa văn sọc giống hệt nhau, giống như dấu vân tay con người.'
  },
  'giraffe': {
    name: 'Giraffe', nameVi: 'Hươu cao cổ', type: 'giraffe',
    origin: 'Châu Phi', size: 'Khổng lồ (4.3-5.7 m cao)', weight: '800-1.200 kg', lifespan: '25-28 năm',
    temperament: 'Hiền lành, tò mò, yên bình, xã hội',
    description: 'Hươu cao cổ là loài động vật cao nhất trên cạn. Cổ dài giúp chúng ăn lá trên cao mà không loài nào khác với tới được.',
    careLevel: 'Bảo tồn (hoang dã)', exercise: 'Cao (đi lại liên tục tìm thức ăn)', grooming: 'Không áp dụng',
    health: 'Bệnh da, ký sinh trùng, vấn đề tim mạch do huyết áp cao', funFact: 'Tim hươu cao cổ nặng khoảng 11 kg và tạo huyết áp gấp đôi con người để bơm máu lên não qua cổ dài 2 mét.'
  },
};

/**
 * Tìm thông tin giống loài theo tên tiếng Anh.
 * Hỗ trợ tìm kiếm gần đúng (partial match).
 */
export function getBreedInfo(breedName: string): BreedInfo | null {
  if (!breedName) return null;

  // Tìm chính xác
  if (BREED_DATABASE[breedName]) return BREED_DATABASE[breedName];

  // Tìm gần đúng
  const lower = breedName.toLowerCase();
  for (const key of Object.keys(BREED_DATABASE)) {
    if (key.toLowerCase() === lower) return BREED_DATABASE[key];
    if (lower.includes(key.toLowerCase()) || key.toLowerCase().includes(lower)) {
      return BREED_DATABASE[key];
    }
  }

  return null;
}
