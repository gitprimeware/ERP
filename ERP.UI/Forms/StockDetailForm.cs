using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ERP.Core.Models;
using ERP.DAL.Repositories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class StockDetailForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dataGridView;
        private MaterialEntryRepository _entryRepository;
        private MaterialExitRepository _exitRepository;
        private OrderRepository _orderRepository;

        // Malzeme listesi (kullanıcının verdiği liste)
        private readonly List<MaterialInfo> _materials = new List<MaterialInfo>
        {
            // Profiller
            new MaterialInfo { MaterialType = "Profil", ProfileType = "S", DisplayName = "Standart Profil" },
            new MaterialInfo { MaterialType = "Profil", ProfileType = "G", DisplayName = "Geniş Profil" },
            // Alüminyum
            new MaterialInfo { MaterialType = "Alüminyum", Size = 214, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 213, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 314, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 314, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 313, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 414, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 414, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 413, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 514, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 514, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 513, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 614, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 614, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 613, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 714, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 714, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 713, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 814, Thickness = 0.120m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 814, Thickness = 0.150m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 813, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 1014, Thickness = 0.165m },
            new MaterialInfo { MaterialType = "Alüminyum", Size = 1013, Thickness = 0.180m },
            // Galvaniz
            new MaterialInfo { MaterialType = "Galvaniz", Size = 251, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 351, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 451, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 551, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 651, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 751, Thickness = 0.85m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 851, Thickness = 1.0m },
            new MaterialInfo { MaterialType = "Galvaniz", Size = 1051, Thickness = 1.0m }
        };

        public StockDetailForm()
        {
            _entryRepository = new MaterialEntryRepository();
            _exitRepository = new MaterialExitRepository();
            _orderRepository = new OrderRepository();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadStockData();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "Stok Ayrıntı",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // DataGridView
            _dataGridView = new DataGridView
            {
                Location = new Point(30, 80),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ThemeColors.Background,
                BorderStyle = BorderStyle.None
            };

            _mainPanel.Resize += (s, e) =>
            {
                _dataGridView.Width = _mainPanel.Width - 60;
                _dataGridView.Height = _mainPanel.Height - 130;
            };

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_dataGridView);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void LoadStockData()
        {
            try
            {
                var entries = _entryRepository.GetAll();
                var exits = _exitRepository.GetAll();
                var orders = _orderRepository.GetAll();

                LoadDataGridView(entries, exits, orders);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stok verileri yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataGridView(List<MaterialEntry> entries, List<MaterialExit> exits, List<Order> orders)
        {
            _dataGridView.DataSource = null;
            _dataGridView.Columns.Clear();

            _dataGridView.AutoGenerateColumns = false;

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaterialDisplay",
                HeaderText = "Malzeme",
                Name = "MaterialDisplay",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Giris",
                HeaderText = "Giriş",
                Name = "Giris",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ToplamSiparisKullanim",
                HeaderText = "Toplam Sipariş Kullanım",
                Name = "ToplamSiparisKullanim",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HurdaCikis",
                HeaderText = "Hurda Çıkış",
                Name = "HurdaCikis",
                Width = 100
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DuzenlemeCikis",
                HeaderText = "Düzenleme Çıkış",
                Name = "DuzenlemeCikis",
                Width = 130
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SiparisSonrasiKalan",
                HeaderText = "Sipariş Sonrası Kalan",
                Name = "SiparisSonrasiKalan",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SevkEdilmisKullanim",
                HeaderText = "Sevk Edilmiş Kullanım",
                Name = "SevkEdilmisKullanim",
                Width = 150
            });

            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UretimDisiKalan",
                HeaderText = "Üretim Dışı Kalan",
                Name = "UretimDisiKalan",
                Width = 150
            });

            var stockData = new List<StockRowData>();

            foreach (var material in _materials)
            {
                var stockRow = CalculateStockForMaterial(material, entries, exits, orders);
                stockData.Add(stockRow);
            }

            _dataGridView.DataSource = stockData;

            // Stil ayarları
            _dataGridView.DefaultCellStyle.BackColor = ThemeColors.Surface;
            _dataGridView.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            _dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _dataGridView.EnableHeadersVisualStyles = false;
        }

        private StockRowData CalculateStockForMaterial(MaterialInfo material, List<MaterialEntry> entries, List<MaterialExit> exits, List<Order> orders)
        {
            string materialDisplay;
            if (material.MaterialType == "Profil")
            {
                materialDisplay = material.DisplayName;
            }
            else if (material.MaterialType == "Alüminyum")
            {
                materialDisplay = $"Alü. {material.Size}X{material.Thickness.ToString("F3", CultureInfo.InvariantCulture).Replace(".", ",")}";
            }
            else // Galvaniz
            {
                materialDisplay = $"Gal. {material.Size}X{material.Thickness.ToString("F2", CultureInfo.InvariantCulture).Replace(".", ",")}";
            }

            // Giriş: Malzeme girişlerinden toplam
            decimal giris = 0;
            if (material.MaterialType == "Profil")
            {
                // Profil için giriş hesaplama (şimdilik 0, gerekirse eklenebilir)
                giris = 0;
            }
            else
            {
                giris = entries
                    .Where(e => e.MaterialType == material.MaterialType && e.Size == material.Size && e.Thickness == material.Thickness)
                    .Sum(e => e.Quantity);
            }

            // Hurda Çıkış: Malzeme çıkışlardan toplam (TransactionType = "Hurda Çıkış")
            decimal hurdaCikis = 0;
            if (material.MaterialType != "Profil")
            {
                hurdaCikis = exits
                    .Where(e => e.MaterialType == material.MaterialType && e.Size == material.Size && e.Thickness == material.Thickness && e.TransactionType == "Hurda Çıkış")
                    .Sum(e => e.Quantity);
            }

            // Düzenleme Çıkış: Malzeme çıkışlardan toplam (TransactionType = "Düzenleme Çıkış")
            decimal duzenlemeCikis = 0;
            if (material.MaterialType != "Profil")
            {
                duzenlemeCikis = exits
                    .Where(e => e.MaterialType == material.MaterialType && e.Size == material.Size && e.Thickness == material.Thickness && e.TransactionType == "Düzenleme Çıkış")
                    .Sum(e => e.Quantity);
            }

            // Toplam Sipariş Kullanım: Sarfiyattaki hesaplamalara göre
            decimal toplamSiparisKullanim = 0;
            decimal sevkEdilmisKullanim = 0;

            if (material.MaterialType == "Profil")
            {
                // Profil için: Profil Ağırlığı = Profil Mode Ağırlığı * 4 * Toplam Adet * (Yükseklik (mm) + Bypass / 1000)
                foreach (var order in orders.Where(o => o.Status != "İptal" && !string.IsNullOrEmpty(o.ProductCode) && o.LamelThickness.HasValue))
                {
                    var productCodeParts = order.ProductCode.Split('-');
                    if (productCodeParts.Length >= 3)
                    {
                        string modelProfile = productCodeParts[2]; // LG veya HS
                        if (modelProfile.Length >= 2)
                        {
                            char profileLetter = modelProfile[1]; // G veya S
                            
                            // Profil tipi eşleşiyor mu?
                            if ((material.ProfileType == "S" && (profileLetter == 'S' || profileLetter == 's')) ||
                                (material.ProfileType == "G" && (profileLetter == 'G' || profileLetter == 'g')))
                            {
                                // Profil Mode Ağırlığı: G=0.5, S=0.3
                                decimal profilModeAgirligi = profileLetter == 'G' || profileLetter == 'g' ? 0.5m : 0.3m;
                                
                                // Yükseklik (mm)
                                int yukseklikMM = 0;
                                if (productCodeParts.Length >= 5 && int.TryParse(productCodeParts[4], out int yukseklik))
                                {
                                    yukseklikMM = yukseklik;
                                }
                                
                                // Bypass ölçüsü
                                decimal bypassOlcusu = 0;
                                if (!string.IsNullOrEmpty(order.BypassSize))
                                {
                                    decimal.TryParse(order.BypassSize.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out bypassOlcusu);
                                }
                                
                                // Plaka ölçüsü (mm)
                                int plakaOlcusuMM = 0;
                                if (productCodeParts.Length >= 4 && int.TryParse(productCodeParts[3], out int plakaOlcusu))
                                {
                                    plakaOlcusuMM = plakaOlcusu;
                                }
                                
                                // Plaka Adet: Plaka ölçüsü <= 1150 ise 1, > 1150 ise 4
                                int plakaAdet = plakaOlcusuMM <= 1150 ? 1 : 4;
                                
                                // Boy Adet: Yükseklik <= 1800 ise 1, > 1800 ise 2
                                int boyAdet = yukseklikMM <= 1800 ? 1 : 2;
                                
                                // Toplam Adet: Sipariş adedi * Boy adet * Plaka adet
                                int toplamAdet = order.Quantity * boyAdet * plakaAdet;
                                
                                // Profil Ağırlığı = Profil Mode Ağırlığı * 4 * Toplam Adet * (Yükseklik (mm) + Bypass / 1000)
                                decimal profilAgirligi = profilModeAgirligi * 4 * toplamAdet * ((yukseklikMM + bypassOlcusu) / 1000m);
                                
                                toplamSiparisKullanim += profilAgirligi;
                                
                                if (order.Status == "Sevkiyata Hazır" || order.Status == "Sevk Edildi")
                                {
                                    sevkEdilmisKullanim += profilAgirligi;
                                }
                            }
                        }
                    }
                }
            }
            else if (material.MaterialType == "Alüminyum")
            {
                // Alüminyum için: Toplam Alüminyum Ağırlığı = Plaka Ağırlığı * Plaka Adedi (hesaplanan)
                foreach (var order in orders.Where(o => o.Status != "İptal" && !string.IsNullOrEmpty(o.ProductCode) && o.LamelThickness.HasValue))
                {
                    var productCodeParts = order.ProductCode.Split('-');
                    if (productCodeParts.Length >= 4)
                    {
                        // Plaka ölçüsü (mm)
                        if (int.TryParse(productCodeParts[3], out int plakaOlcusuMM))
                        {
                            int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                            decimal plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                            
                            // Lamel kalınlığı (plaka kalınlığı)
                            decimal lamelKalinligi = order.LamelThickness.Value;
                            
                            // Alüminyum malzeme eşleştirmesi: Üretim Plaka Ölçüsü (cm) -> Alü koduna
                            // Örnek: 60 cm -> 613 veya 614'e denk geliyor
                            // X'den sonraki değer: Plaka Kalınlığı -> kalınlığa eşleştir
                            int materialSize = material.Size;
                            int materialSizeFirst = materialSize / 100; // 613 -> 6
                            int materialSizeLast = materialSize % 10; // 613 -> 3, 614 -> 4
                            
                            // Plaka ölçüsü cm'den malzeme koduna eşleştir
                            int plakaOlcusuInt = (int)Math.Round(plakaOlcusuCM);
                            int plakaOlcusuFirst = plakaOlcusuInt / 10; // 60 -> 6
                            int plakaOlcusuLast = plakaOlcusuInt % 10; // 60 -> 0, 61 -> 1
                            
                            // Eşleşme kontrolü: 60 cm -> 613 veya 614'e denk geliyor
                            bool sizeMatches = false;
                            if (plakaOlcusuFirst == materialSizeFirst)
                            {
                                // Son rakam kontrolü: 60 -> 3 veya 4, 61 -> 3 veya 4
                                if (plakaOlcusuLast == 0 && (materialSizeLast == 3 || materialSizeLast == 4))
                                    sizeMatches = true;
                                else if (plakaOlcusuLast == 1 && (materialSizeLast == 3 || materialSizeLast == 4))
                                    sizeMatches = true;
                            }
                            
                            // Kalınlık eşleşmesi
                            bool thicknessMatches = Math.Abs(lamelKalinligi - material.Thickness) < 0.001m;
                            
                            if (sizeMatches && thicknessMatches)
                            {
                                // Plaka ağırlığı hesapla
                                decimal plakaAgirligi = CalculatePlakaAgirligi(plakaOlcusuCM, lamelKalinligi);
                                
                                if (plakaAgirligi > 0)
                                {
                                    // Model harfi (H, D, M, L)
                                    char modelLetter = 'H';
                                    if (productCodeParts.Length >= 3)
                                    {
                                        string modelProfile = productCodeParts[2];
                                        if (modelProfile.Length > 0)
                                        {
                                            modelLetter = modelProfile[0];
                                        }
                                    }
                                    
                                    // Hatve değerini al
                                    decimal hatve = GetHtave(modelLetter);
                                    
                                    // Yükseklik (mm)
                                    int yukseklikMM = 0;
                                    if (productCodeParts.Length >= 5 && int.TryParse(productCodeParts[4], out int yukseklik))
                                    {
                                        yukseklikMM = yukseklik;
                                    }
                                    
                                    // Yeni formül: Plaka Adedi = Math.Ceiling(Yükseklik (mm) / hatve) * Sipariş Adedi
                                    decimal birimPlakaAdedi = hatve > 0 ? (decimal)yukseklikMM / hatve : 0;
                                    decimal birimPlakaAdediYuvarlanmis = Math.Ceiling(birimPlakaAdedi);
                                    decimal plakaAdedi = birimPlakaAdediYuvarlanmis * order.Quantity;
                                    
                                    // Toplam Alüminyum Ağırlığı = Plaka Ağırlığı * Plaka Adedi
                                    decimal toplamAluminyumAgirligi = plakaAgirligi * Math.Ceiling(plakaAdedi);
                                    
                                    toplamSiparisKullanim += toplamAluminyumAgirligi;
                                    
                                    if (order.Status == "Sevkiyata Hazır" || order.Status == "Sevk Edildi")
                                    {
                                        sevkEdilmisKullanim += toplamAluminyumAgirligi;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (material.MaterialType == "Galvaniz")
            {
                // Galvaniz için: Toplam Galvaniz Ağırlığı = Galvaniz Kapak Ağırlığı * Kapak Adedi
                foreach (var order in orders.Where(o => o.Status != "İptal" && !string.IsNullOrEmpty(o.ProductCode)))
                {
                    var productCodeParts = order.ProductCode.Split('-');
                    if (productCodeParts.Length >= 4)
                    {
                        if (int.TryParse(productCodeParts[3], out int plakaOlcusuMM))
                        {
                            int plakaOlcusuComMM = plakaOlcusuMM <= 1150 ? plakaOlcusuMM : plakaOlcusuMM / 2;
                            decimal plakaOlcusuCM = plakaOlcusuComMM / 10.0m;
                            
                            // Galvaniz malzeme eşleştirmesi: Üretim Plaka Ölçüsü (cm) -> Gal koduna
                            // Örnek: 50 cm -> 551'e denk geliyor
                            int materialSize = material.Size;
                            int materialSizeFirst = materialSize / 100; // 551 -> 5
                            int materialSizeMiddle = (materialSize / 10) % 10; // 551 -> 5
                            
                            // Plaka ölçüsü cm'den malzeme koduna eşleştir
                            int plakaOlcusuInt = (int)Math.Round(plakaOlcusuCM);
                            int plakaOlcusuFirst = plakaOlcusuInt / 10; // 50 -> 5
                            
                            // Eşleşme kontrolü: 50 cm -> 551'e denk geliyor
                            bool sizeMatches = (plakaOlcusuFirst == materialSizeFirst && materialSizeMiddle == 5);
                            
                            if (sizeMatches)
                            {
                                // Galvaniz kapak ağırlığı hesapla
                                decimal galvanizKapakAgirligi = CalculateGalvanizKapakAgirligi(plakaOlcusuCM);
                                
                                if (galvanizKapakAgirligi > 0)
                                {
                                    // Kapak adedi: Sipariş adedi
                                    int kapakAdedi = order.Quantity;
                                    
                                    // Toplam Galvaniz Ağırlığı = Galvaniz Kapak Ağırlığı * Kapak Adedi
                                    decimal toplamGalvanizAgirligi = galvanizKapakAgirligi * kapakAdedi;
                                    
                                    toplamSiparisKullanim += toplamGalvanizAgirligi;
                                    
                                    if (order.Status == "Sevkiyata Hazır" || order.Status == "Sevk Edildi")
                                    {
                                        sevkEdilmisKullanim += toplamGalvanizAgirligi;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Sipariş Sonrası Kalan = Giriş - Toplam Sipariş Kullanım - Hurda Çıkış - Düzenleme Çıkış
            decimal siparisSonrasiKalan = giris - toplamSiparisKullanim - hurdaCikis - duzenlemeCikis;

            // Üretim Dışı Kalan = Giriş - Sevk Edilmiş - Düzenleme Çıkış - Hurda
            decimal uretimDisiKalan = giris - sevkEdilmisKullanim - duzenlemeCikis - hurdaCikis;

            return new StockRowData
            {
                MaterialDisplay = materialDisplay,
                Giris = giris.ToString("F3", CultureInfo.InvariantCulture),
                ToplamSiparisKullanim = toplamSiparisKullanim.ToString("F3", CultureInfo.InvariantCulture),
                HurdaCikis = hurdaCikis.ToString("F3", CultureInfo.InvariantCulture),
                DuzenlemeCikis = duzenlemeCikis.ToString("F3", CultureInfo.InvariantCulture),
                SiparisSonrasiKalan = siparisSonrasiKalan.ToString("F3", CultureInfo.InvariantCulture),
                SevkEdilmisKullanim = sevkEdilmisKullanim.ToString("F3", CultureInfo.InvariantCulture),
                UretimDisiKalan = uretimDisiKalan.ToString("F3", CultureInfo.InvariantCulture)
            };
        }

        private decimal CalculatePlakaAgirligi(decimal plakaOlcusuCM, decimal aluminyumKalinligi)
        {
            var normalizedSize = Math.Round(plakaOlcusuCM, 1, MidpointRounding.AwayFromZero);
            var normalizedThickness = Math.Round(aluminyumKalinligi, 3, MidpointRounding.AwayFromZero);

            if (normalizedSize >= 18 && normalizedSize <= 24)
            {
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.019m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.014m;
            }
            if (normalizedSize >= 28 && normalizedSize <= 34)
            {
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.042m;
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.038m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.031m;
            }
            if (normalizedSize >= 38 && normalizedSize <= 44)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.068m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.074m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.054m;
            }
            if (normalizedSize >= 48 && normalizedSize <= 54)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.105m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.115m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.085m;
            }
            if (normalizedSize >= 58 && normalizedSize <= 64)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.150m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.164m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.120m;
            }
            if (normalizedSize >= 68 && normalizedSize <= 74)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.205m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.224m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.165m;
            }
            if (normalizedSize >= 78 && normalizedSize <= 84)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.270m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.295m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.218m;
            }
            if (normalizedSize >= 88 && normalizedSize <= 94)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.345m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.377m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.279m;
            }
            if (normalizedSize >= 98 && normalizedSize <= 104)
            {
                if (ThicknessMatches(normalizedThickness, 0.15m)) return 0.430m;
                if (ThicknessMatches(normalizedThickness, 0.165m)) return 0.470m;
                if (ThicknessMatches(normalizedThickness, 0.12m)) return 0.348m;
            }

            return 0m;
        }

        private bool ThicknessMatches(decimal actual, decimal expected)
        {
            return Math.Abs(actual - expected) <= 0.0005m;
        }

        private decimal CalculateGalvanizKapakAgirligi(decimal plakaOlcusuCM)
        {
            var normalizedSize = Math.Round(plakaOlcusuCM, 1, MidpointRounding.AwayFromZero);

            if (normalizedSize >= 18 && normalizedSize <= 24) return 0.421m;
            if (normalizedSize >= 28 && normalizedSize <= 34) return 0.663m;
            if (normalizedSize >= 38 && normalizedSize <= 44) return 1.358m;
            if (normalizedSize >= 48 && normalizedSize <= 54) return 2.026m;
            if (normalizedSize >= 58 && normalizedSize <= 64) return 2.828m;
            if (normalizedSize >= 68 && normalizedSize <= 74) return 3.764m;
            if (normalizedSize >= 78 && normalizedSize <= 84) return 5.5685m;
            if (normalizedSize >= 98 && normalizedSize <= 104) return 8.672m;

            return 0m;
        }

        private int GetPlakaAdedi10cm(char modelLetter)
        {
            switch (char.ToUpper(modelLetter))
            {
                case 'H': return 32;
                case 'D': return 24;
                case 'M': return 17;
                case 'L': return 12;
                default: return 0;
            }
        }

        private decimal GetHtave(char modelLetter)
        {
            switch (char.ToUpper(modelLetter))
            {
                case 'H': return 3.25m;
                case 'D': return 4.5m;
                case 'M': return 6.5m;
                case 'L': return 9m;
                default: return 0m;
            }
        }

        private class MaterialInfo
        {
            public string MaterialType { get; set; }
            public int Size { get; set; }
            public decimal Thickness { get; set; }
            public string ProfileType { get; set; } // S veya G
            public string DisplayName { get; set; } // Profil için görünen isim
        }

        private class StockRowData
        {
            public string MaterialDisplay { get; set; }
            public string Giris { get; set; }
            public string ToplamSiparisKullanim { get; set; }
            public string HurdaCikis { get; set; }
            public string DuzenlemeCikis { get; set; }
            public string SiparisSonrasiKalan { get; set; }
            public string SevkEdilmisKullanim { get; set; }
            public string UretimDisiKalan { get; set; }
        }
    }
}

