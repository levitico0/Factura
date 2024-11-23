using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UspgPOS.Data;
using UspgPOS.Models;

namespace UspgPOS.Controllers
{
	public class VentasController : Controller
	{
		private readonly AppDbContext _context;

		public VentasController(AppDbContext context)
		{
			_context = context;
		}

		
		public async Task<IActionResult> Index()
		{
			var appDbContext = _context.Ventas.Include(v => v.Cliente).Include(v => v.Sucursal);
			return View(await appDbContext.ToListAsync());
		}

		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var venta = await _context.Ventas
				.Include(v => v.Cliente)
				.Include(v => v.Sucursal)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (venta == null)
			{
				return NotFound();
			}

			return View(venta);
		}

		public IActionResult Create()
		{
			ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
			ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", HttpContext.Session.GetString("SucursalId"));
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Fecha,Total,ClienteId,SucursalId")] Venta venta)
		{
			if (ModelState.IsValid)
			{
				_context.Add(venta);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
			ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
			return View(venta);
		}

	
		public async Task<IActionResult> Edit(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var venta = await _context.Ventas.FindAsync(id);
			if (venta == null)
			{
				return NotFound();
			}
			ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
			ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
			return View(venta);
		}

	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(long? id, [Bind("Id,Fecha,Total,ClienteId,SucursalId")] Venta venta)
		{
			if (id != venta.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(venta);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!VentaExists(venta.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
			ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
			return View(venta);
		}

		
		public async Task<IActionResult> Delete(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var venta = await _context.Ventas
				.Include(v => v.Cliente)
				.Include(v => v.Sucursal)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (venta == null)
			{
				return NotFound();
			}

			return View(venta);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(long? id)
		{
			var venta = await _context.Ventas.FindAsync(id);
			if (venta != null)
			{
				_context.Ventas.Remove(venta);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool VentaExists(long? id)
		{
			return _context.Ventas.Any(e => e.Id == id);
		}

			public IActionResult ImprimirFactura(long id)
			{
				QuestPDF.Settings.License = LicenseType.Community;

				var venta = _context.Ventas
					.Include(v => v.Cliente)
					.Include(v => v.Sucursal)
					.Include(v => v.DetallesVenta)
						.ThenInclude(d => d.Producto)
					.FirstOrDefault(v => v.Id == id);

				if (venta == null)
				{
					return NotFound();
				}

				var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/icons/apple-touch-icon-180x180.png");
				var monedaGuatemala = new System.Globalization.CultureInfo("es-GT");

				var document = Document.Create(container =>
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(1, Unit.Centimetre);


						page.Header().Row(header =>
						{
							header.AutoItem().Text($"Fecha: {venta.Fecha.ToString("dd/MM/yyyy")} ");
                            header.RelativeItem().Text("FACTURA\nCOMERCIAL")
      .FontSize(24)
      .Bold()
      .AlignCenter();


                            header.ConstantItem(80).Image(logo, ImageScaling.FitArea);

						});

						page.Content().Column(column =>
						{

							column.Item().Row(row =>
							{
								row.RelativeItem().Text($"Nombre: {venta.Cliente?.Nombre}");
							});

							column.Item().Row(row =>
							{
								row.RelativeItem().Text($"Direccion: .________________________.");
								row.RelativeItem().Text($"Telfono: .________________________.");
                         
                            });



							column.Item().Row(row =>
							{
								row.RelativeItem().Text($"Sucursal: {venta.Sucursal?.Nombre}");
							});

							column.Item().PaddingVertical(1, Unit.Centimetre);

							column.Item().Table(table =>
							{
								table.ColumnsDefinition(columns =>
								{
									columns.RelativeColumn();
									columns.RelativeColumn();
									columns.RelativeColumn();
									columns.RelativeColumn();
			
								});

								table.Header(header =>
								{
									string fontColor = "#09506d";

									header.Cell().Element(EstiloCelda).Text("CANTIDAD").FontColor(fontColor).FontSize(12).Bold();
									header.Cell().Element(EstiloCelda).Text("DESCRIPCION").FontColor(fontColor).FontSize(12).Bold();
									header.Cell().Element(EstiloCelda).Text("UNIDAD").FontColor(fontColor).FontSize(12).Bold();
									header.Cell().Element(EstiloCelda).Text("TOTAL").FontColor(fontColor).FontSize(12).Bold();
                                    header.Cell().Element(EstiloCelda).Text("-").FontColor(fontColor).FontSize(1).Bold();
                                    header.Cell().Element(EstiloCelda).Text("-").FontColor(fontColor).FontSize(1).Bold();
                                    header.Cell().Element(EstiloCelda).Text("-").FontColor(fontColor).FontSize(1).Bold();
                                    header.Cell().Element(EstiloCelda).Text("-").FontColor(fontColor).FontSize(1).Bold();



                                    static IContainer EstiloCelda(IContainer container)
									{
                                        return container.Background("#ff0000").Border(1).BorderColor("#ff0000").Padding(5).AlignCenter(); // Color rojo
                                    }

                                });


                                var detalles = venta.DetallesVenta.Take(7).ToList();
                                while (detalles.Count < 7)
                                {
                                    detalles.Add(new DetalleVenta());
                                }

                                foreach (var detalle in detalles)
                                {
                                    table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignCenter().Text(detalle.Cantidad > 0 ? detalle.Cantidad.ToString() : "");
                                    table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignCenter().Text(detalle.Producto?.Nombre ?? "");
                                    table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignCenter().Text(detalle.PrecioUnitario > 0 ? detalle.PrecioUnitario.ToString("C", monedaGuatemala) : "");
                                    table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignCenter().Text(detalle.Cantidad * detalle.PrecioUnitario > 0 ? (detalle.Cantidad * detalle.PrecioUnitario).ToString("C", monedaGuatemala) : "");
                                }



                            });

							column.Item().PaddingVertical(1, Unit.Centimetre);

							column.Item().Row(row =>
							{

								column.Item().Column(col =>
								{

									column.Item().AlignRight().MaxWidth(150).Table(table =>
									{
										table.ColumnsDefinition(columns =>
										{
											columns.RelativeColumn();
											columns.RelativeColumn();
                                        

                                        });

                                        table.Cell()
                                           .Background("#ff0000")  
                                           .Border(1)
                                           .BorderColor("#ff0000")
                                           .Padding(5)
                                           .AlignRight()
                                           .Text("TOTAL")
                                           .FontSize(12)
                                           .Bold();

                                        table.Cell()
                                        .Background("#ff0000")  
                                        .Border(1)
                                        .BorderColor("#ff0000")
                                        .Padding(5)
                                        .AlignRight()
                                        .Text(venta.Total.ToString("C", monedaGuatemala))
                                        .FontSize(12);
                                    });
                                });
                                column.Item().Row(row =>
								{
									row.RelativeItem().Text($"Direccin: {venta.Sucursal?.Ciudad}, {venta.Sucursal?.Area}");
									row.RelativeItem().Text($"Macdonasl☺");
								});

								column.Item().Row(row =>
								{
									row.RelativeItem().Text($"MacDonals.com");
									row.RelativeItem().Text($"(502) 485412-55434");
								});


							});

							column.Item().PaddingVertical(40);

                            column.Item().AlignLeft().PaddingBottom(20).Text("Servicio a domicilio")
                                  .FontSize(20).Italic().FontColor("#606060");

                       
                            column.Item().AlignRight().Column(row =>
                            {
                                row.Item().Text($".____________________________________.").FontSize(10).Bold();
                                row.Item().AlignRight().Text($"FIRMA CLIENTE").FontSize(20).Bold();
                            });
                        });

                    });
				});

				var stream = new MemoryStream();
				document.GeneratePdf(stream);
				stream.Position = 0;

				return File(stream, "application/pdf", $"Factura_{id}.pdf");
			}

		}
	}