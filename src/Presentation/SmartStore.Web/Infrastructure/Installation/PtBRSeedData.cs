using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Media;
using SmartStore.Data.Setup;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Web.Infrastructure.Installation
{

    public class PtBRSeedData : InvariantSeedData
    {
        public PtBRSeedData()
        {
        }

        protected override void Alter(IList<ISettings> settings)
        {
            base.Alter(settings);

            settings
                .Alter<ContentSliderSettings>(x =>
                {
                    var slidePics = base.DbContext.Set<Picture>().ToList();

                    var slide1PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-1")).First().Id;
                    var slide2PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-2")).First().Id;
                    var slide3PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-3")).First().Id;

                    //slide 1
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 1,
                        //LanguageName = "English",
                        Title = "A melhor coisa que poderia acontecer para o iPhone",
                        Text = @"<ul>
                                    <li>Mais fino, design mais leve</li>
                                    <li>4"" display de retina.</li>
                                    <li>Dados móveis ultra-rápidos.</li>                       
                                </ul>",
                        Published = true,
                        LanguageCulture = "pt-BR",
                        PictureId = slide1PicId,
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "mais...",
                            Type = "btn-primary",
                            Url = "~/iphone-5"
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "Comprar agora",
                            Type = "btn-danger",
                            Url = "~/iphone-5"
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                    //slide 2
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 2,
                        //LanguageName = "English",
                        Title = "Compre música online!",
                        Text = @"<p>Compre aqui e faça o download de uma só vez.</p>
                                 <p>Melhor qualidade a 320 kbit/s.</p>
                                 <p>Amostras e download em alta velocidade.</p>",
                        Published = true,
                        LanguageCulture = "pt-BR",
                        PictureId = slide2PicId,
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "mais...",
                            Type = "btn-warning",
                            Url = "~/musik-kaufen-sofort-herunterladen"
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = false
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                    //slide 3
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 3,
                        //LanguageName = "English",
                        Title = "Pronto para revolução?",
                        Text = @"<p>ThinkStore.NET é a nova solução dinâmica E-Commerce da Think A.M..</p>
                                 <ul>
                                     <li>Ordem de Serviço, clientes e gestão de estoque.</li>
                                     <li>SEO otimizado | 100% otimizado para aparelhos móveis.</li>
                                     <li>Avaliações &amp; Classificações | Importação de dados.</li>
                                 </ul>",
                        Published = true,
                        LanguageCulture = "pt-BR",
                        PictureId = slide3PicId,
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "e muito mais...",
                            Type = "btn-success",
                            Url = "http://www.Thinkam.net/thinkstore"
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = false
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                });
        }
    }
}
