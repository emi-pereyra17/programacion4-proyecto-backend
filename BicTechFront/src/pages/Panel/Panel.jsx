import React, { useEffect, useState } from "react";
import FormAgregarProducto from "../../components/AgregarProducto/FormAgregarProducto";
import FormAgregarCategoria from "../../components/AgregarCategoria/FormAgregarCategoria";
import CardCategoria from "../../components/CardCategoria/CardCategoria";
import CardModificarCategoria from "../../components/CardModificarCategoria/CardModificarCategoria";
import CardMarca from "../../components/CardMarca/CardMarca";
import CardModificarMarca from "../../components/CardModificarMarca/CardModificarMarca";
import FormAgregarMarca from "../../components/AgregarMarca/FormAgregarMarca";
import CardCategoriaMarca from "../../components/CardCategoriaMarca/CardCategoriaMarca";
import FormAgregarRelacion from "../../components/AgregarRelacion/FormAgregarRelacion";
import CardUsuario from "../../components/CardUsuario/CardUsuario";
import CardPedido from "../../components/CardPedido/CardPedido";
import { toast } from "react-toastify";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

const ESTADOS_ENUM = [
  { label: "Pendiente", value: 0 },
  { label: "Enviado", value: 1 },
  { label: "Entregado", value: 2 },
  { label: "Cancelado", value: 3 },
];

const Panel = () => {
  const [marcas, setMarcas] = useState([]);
  const [totalMarcas, setTotalMarcas] = useState(0);
  const [marcaPage, setMarcaPage] = useState(1);
  const [marcaPageSize, setMarcaPageSize] = useState(10);
  const [categorias, setCategorias] = useState([]);
  const [totalCategorias, setTotalCategorias] = useState(0);
  const [categoriaPage, setCategoriaPage] = useState(1);
  const [categoriaPageSize, setCategoriaPageSize] = useState(10);
  const [relaciones, setRelaciones] = useState([]);
  const [totalRelaciones, setTotalRelaciones] = useState(0);
  const [relacionPage, setRelacionPage] = useState(1);
  const [relacionPageSize, setRelacionPageSize] = useState(10);
  const [categoriaAModificar, setCategoriaAModificar] = useState(null);
  const [marcaAModificar, setMarcaAModificar] = useState(null);
  const [usuarios, setUsuarios] = useState([]);
  const [totalUsuarios, setTotalUsuarios] = useState(0);
  const [usuarioPage, setUsuarioPage] = useState(1);
  const [usuarioPageSize, setUsuarioPageSize] = useState(10);
  const [pedidos, setPedidos] = useState([]);
  const [totalPedidos, setTotalPedidos] = useState(0);
  const [pedidoPage, setPedidoPage] = useState(1);
  const [pedidoPageSize, setPedidoPageSize] = useState(10);
  const [paises, setPaises] = useState([]);

  useEffect(() => {
    fetch(`${API_URL}/paises`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
    })
      .then((res) => res.json())
      .then((data) => {
        setPaises(data.paises || []);
      });
  }, []);
  useEffect(() => {
    fetch(
      `${API_URL}/categorias/paginado?page=${categoriaPage}&pageSize=${categoriaPageSize}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        setCategorias(data.categorias || []);
        setTotalCategorias(data.total || 0);
      });
  }, [categoriaPage, categoriaPageSize]);

  useEffect(() => {
    fetch(
      `${API_URL}/marcas/paginado?page=${marcaPage}&pageSize=${marcaPageSize}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        setMarcas(data.marcas || []);
        setTotalMarcas(data.total || 0);
      });
  }, [marcaPage, marcaPageSize]);

  useEffect(() => {
    fetch(
      `${API_URL}/categoriaMarca/paginado?page=${relacionPage}&pageSize=${relacionPageSize}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        setRelaciones(data.cms || []);
        setTotalRelaciones(data.total || 0);
      });
  }, [relacionPage, relacionPageSize]);

  useEffect(() => {
    fetch(
      `${API_URL}/usuarios/paginado?page=${usuarioPage}&pageSize=${usuarioPageSize}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        setUsuarios(data.usuarios || []);
        setTotalUsuarios(data.total || 0);
      });
  }, [usuarioPage, usuarioPageSize]);

  useEffect(() => {
    fetch(
      `${API_URL}/pedidos/paginado?page=${pedidoPage}&pageSize=${pedidoPageSize}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    )
      .then((res) => res.json())
      .then((data) => {
        console.log("Pedidos:", data.pedidos);
        setPedidos(data.pedidos || []);
        setTotalPedidos(data.total || 0);
      });
  }, [pedidoPage, pedidoPageSize]);

  const handleEliminarCategoria = (categoria) => {
    toast.info(
      <div>
        ¿Deseas eliminar la categoría <b>{categoria.nombre}</b>?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={async () => {
              toast.dismiss();
              try {
                const res = await fetch(
                  `${API_URL}/categorias/${categoria.id}`,
                  {
                    method: "DELETE",
                    headers: {
                      Authorization: `Bearer ${localStorage.getItem("token")}`,
                    },
                  }
                );
                if (res.ok) {
                  // Recarga la página actual de categorías
                  fetch(
                    `${API_URL}/categorias/paginado?page=${categoriaPage}&pageSize=${categoriaPageSize}`,
                    {
                      headers: {
                        Authorization: `Bearer ${localStorage.getItem(
                          "token"
                        )}`,
                      },
                    }
                  )
                    .then((res) => res.json())
                    .then((data) => {
                      setCategorias(data.categorias || []);
                      setTotalCategorias(data.total || 0);
                    });
                  toast.success("Categoría eliminada");
                } else {
                  toast.error("No se pudo eliminar la categoría.");
                }
              } catch {
                toast.error("Error de conexión al eliminar.");
              }
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  const handleModificarCategoria = (categoria) => {
    toast.info(
      <div>
        ¿Deseas modificar la categoría <b>{categoria.nombre}</b>?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => {
              toast.dismiss();
              setCategoriaAModificar(categoria);
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  const handleEliminarMarca = (marca) => {
    toast.info(
      <div>
        ¿Deseas eliminar la marca <b>{marca.nombre}</b>?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={async () => {
              toast.dismiss();
              try {
                const res = await fetch(`${API_URL}/marcas/${marca.id}`, {
                  method: "DELETE",
                  headers: {
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                  },
                });
                if (res.ok) {
                  fetch(
                    `${API_URL}/marcas/paginado?page=${marcaPage}&pageSize=${marcaPageSize}`,
                    {
                      headers: {
                        Authorization: `Bearer ${localStorage.getItem(
                          "token"
                        )}`,
                      },
                    }
                  )
                    .then((res) => res.json())
                    .then((data) => {
                      setMarcas(data.marcas || []);
                      setTotalMarcas(data.total || 0);
                    });
                  toast.success("Marca eliminada");
                } else {
                  toast.error("No se pudo eliminar la marca.");
                }
              } catch {
                toast.error("Error de conexión al eliminar.");
              }
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  const handleModificarMarca = (marca) => {
    toast.info(
      <div>
        ¿Deseas modificar la marca <b>{marca.nombre}</b>?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => {
              toast.dismiss();
              setMarcaAModificar(marca);
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  const handleCambiarEstadoPedido = async (pedido, nuevoEstadoLabel) => {
    const estadoObj = ESTADOS_ENUM.find((e) => e.label === nuevoEstadoLabel);
    const nuevoEstadoValue = estadoObj ? estadoObj.value : 0;

    try {
      const res = await fetch(`${API_URL}/pedidos/${pedido.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          ...pedido,
          estado: nuevoEstadoValue,
          usuarioId: pedido.usuarioId,
          direccionEnvio: pedido.direccionEnvio,
          productos: pedido.pedidosDetalles?.map((det) => ({
            productoId: det.productoId,
            cantidad: det.cantidad,
            precio: det.precio,
          })),
        }),
      });
      if (res.ok) {
        toast.success("Estado actualizado");
        fetch(
          `${API_URL}/pedidos/paginado?page=${pedidoPage}&pageSize=${pedidoPageSize}`,
          {
            headers: {
              Authorization: `Bearer ${localStorage.getItem("token")}`,
            },
          }
        )
          .then((res) => res.json())
          .then((data) => {
            setPedidos(data.pedidos || []);
            setTotalPedidos(data.total || 0);
          });
      } else {
        toast.error("No se pudo actualizar el estado");
      }
    } catch {
      toast.error("Error de conexión al actualizar estado");
    }
  };

  const handleEliminarRelacion = (relacion) => {
    toast.info(
      <div>
        ¿Deseas eliminar la relación <b>#{relacion.id}</b>?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={async () => {
              toast.dismiss();
              try {
                const res = await fetch(
                  `${API_URL}/categoriaMarca/${relacion.id}`,
                  {
                    method: "DELETE",
                    headers: {
                      Authorization: `Bearer ${localStorage.getItem("token")}`,
                    },
                  }
                );
                if (res.ok) {
                  fetch(
                    `${API_URL}/categoriaMarca/paginado?page=${relacionPage}&pageSize=${relacionPageSize}`,
                    {
                      headers: {
                        Authorization: `Bearer ${localStorage.getItem(
                          "token"
                        )}`,
                      },
                    }
                  )
                    .then((res) => res.json())
                    .then((data) => {
                      setRelaciones(data.cms || []);
                      setTotalRelaciones(data.total || 0);
                    });
                  toast.success("Relación eliminada");
                } else {
                  toast.error("No se pudo eliminar la relación.");
                }
              } catch {
                toast.error("Error de conexión al eliminar.");
              }
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  const handleEliminarUsuario = (usuario) => {
    toast.info(
      <div>
        ¿Deseas eliminar el usuario{" "}
        <b>
          {usuario.nombre} {usuario.apellido}
        </b>
        ?
        <div style={{ marginTop: 10, display: "flex", gap: 10 }}>
          <button
            style={{
              background: "#ff4d4f",
              color: "#fff",
              border: "none",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={async () => {
              toast.dismiss();
              try {
                const res = await fetch(`${API_URL}/usuarios/${usuario.id}`, {
                  method: "DELETE",
                  headers: {
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                  },
                });
                const data = await res.json().catch(() => ({}));
                if (res.ok) {
                  fetch(
                    `${API_URL}/usuarios/paginado?page=${usuarioPage}&pageSize=${usuarioPageSize}`,
                    {
                      headers: {
                        Authorization: `Bearer ${localStorage.getItem(
                          "token"
                        )}`,
                      },
                    }
                  )
                    .then((res) => res.json())
                    .then((data) => {
                      setUsuarios(data.usuarios || []);
                      setTotalUsuarios(data.total || 0);
                    });
                  toast.success("Usuario eliminado");
                } else {
                  toast.error("No se pudo eliminar el usuario.");
                  console.error("Error al eliminar usuario:", data);
                }
              } catch {
                toast.error("Error de conexión al eliminar.");
              }
            }}
          >
            Sí
          </button>
          <button
            style={{
              background: "#ffe066",
              color: "#222",
              border: "1px solid #bfa100",
              padding: "5px 16px",
              borderRadius: 5,
              cursor: "pointer",
              fontWeight: "bold",
            }}
            onClick={() => toast.dismiss()}
          >
            No
          </button>
        </div>
      </div>,
      { autoClose: false }
    );
  };

  return (
    <>
      {categoriaAModificar && (
        <CardModificarCategoria
          categoria={categoriaAModificar}
          onClose={() => setCategoriaAModificar(null)}
          onCategoriaModificada={(catActualizada) => {
            setCategorias((prev) =>
              prev.map((cat) =>
                cat.id === catActualizada.id ? catActualizada : cat
              )
            );
          }}
        />
      )}
      {marcaAModificar && (
        <CardModificarMarca
          marca={marcaAModificar}
          paises={paises}
          onClose={() => setMarcaAModificar(null)}
          onMarcaModificada={(marcaActualizada) => {
            setMarcas((prev) =>
              prev.map((m) =>
                m.id === marcaActualizada.id ? marcaActualizada : m
              )
            );
            setMarcaAModificar(null);
          }}
        />
      )}
      <div
        className="panel-container"
        style={{
          minHeight: "80vh",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          textAlign: "center",
          background: "rgb(248, 232, 139)",
          paddingTop: "3rem",
        }}
      >
        <h1
          style={{
            fontSize: "3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Panel de Administración
        </h1>
        <p
          style={{
            fontSize: "1.25rem",
            color: "#7c6f00",
            background: "#fffde7",
            padding: "1.5rem 2.5rem",
            borderRadius: "1rem",
            boxShadow: "0 4px 24px 0 rgba(191,161,0,0.08)",
            maxWidth: "800px",
            width: "600px",
            marginTop: "0",
          }}
        >
          Bienvenido al panel de administración.
          <br />
          Aquí puedes gestionar los productos, usuarios y más.
        </p>

        <br />
        <br />
        <br />

        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />
        <br />
        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Pedidos
        </h1>
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Todos los pedidos:
        </h1>
        <div
          style={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          {pedidos.map((pedido) => (
            <CardPedido
              key={pedido.id}
              pedido={pedido}
              onCambiarEstado={handleCambiarEstadoPedido}
            />
          ))}
        </div>
        <div style={{ margin: "1rem 0" }}>
          <button
            disabled={pedidoPage === 1}
            onClick={() => setPedidoPage((prev) => prev - 1)}
          >
            Anterior
          </button>
          <span style={{ margin: "0 1rem" }}>
            Página {pedidoPage} de {Math.ceil(totalPedidos / pedidoPageSize)}
          </span>
          <button
            disabled={pedidoPage * pedidoPageSize >= totalPedidos}
            onClick={() => setPedidoPage((prev) => prev + 1)}
          >
            Siguiente
          </button>
          <select
            value={pedidoPageSize}
            onChange={(e) => {
              setPedidoPageSize(Number(e.target.value));
              setPedidoPage(1);
            }}
            style={{ marginLeft: "1rem" }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </div>

        <br />
        <br />
        <br />
        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />

        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Productos
        </h1>
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Agregar nuevo Producto:
        </h1>

        <FormAgregarProducto marcas={marcas} categorias={categorias} />

        <br />
        <br />
        <br />

        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />

        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Categorías
        </h1>
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Todas las categorías:
        </h1>
        <div
          style={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          {categorias.map((categoria) => (
            <CardCategoria
              key={categoria.id}
              categoria={categoria}
              onEliminar={() => handleEliminarCategoria(categoria)}
              onModificar={setCategoriaAModificar}
            />
          ))}
        </div>

        <div style={{ margin: "1rem 0" }}>
          <button
            disabled={categoriaPage === 1}
            onClick={() => setCategoriaPage((prev) => prev - 1)}
          >
            Anterior
          </button>
          <span style={{ margin: "0 1rem" }}>
            Página {categoriaPage} de{" "}
            {Math.ceil(totalCategorias / categoriaPageSize)}
          </span>
          <button
            disabled={categoriaPage * categoriaPageSize >= totalCategorias}
            onClick={() => setCategoriaPage((prev) => prev + 1)}
          >
            Siguiente
          </button>
          <select
            value={categoriaPageSize}
            onChange={(e) => {
              setCategoriaPageSize(Number(e.target.value));
              setCategoriaPage(1);
            }}
            style={{ marginLeft: "1rem" }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </div>

        <br />
        <br />

        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Agregar nueva Categoría:
        </h1>

        <FormAgregarCategoria
          onCategoriaAgregada={(nuevaCategoria) => {
            setCategorias((prev) => [...prev, nuevaCategoria]);
          }}
        />

        <br />
        <br />

        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />

        <br />

        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Marcas
        </h1>
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Todas las marcas:
        </h1>
        <div
          style={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          {marcas.map((marca) => (
            <CardMarca
              key={marca.id}
              marca={marca}
              onEliminar={() => handleEliminarMarca(marca)}
              onModificar={setMarcaAModificar}
            />
          ))}
        </div>
        <div style={{ margin: "1rem 0" }}>
          <button
            disabled={marcaPage === 1}
            onClick={() => setMarcaPage((prev) => prev - 1)}
          >
            Anterior
          </button>
          <span style={{ margin: "0 1rem" }}>
            Página {marcaPage} de {Math.ceil(totalMarcas / marcaPageSize)}
          </span>
          <button
            disabled={marcaPage * marcaPageSize >= totalMarcas}
            onClick={() => setMarcaPage((prev) => prev + 1)}
          >
            Siguiente
          </button>
          <select
            value={marcaPageSize}
            onChange={(e) => {
              setMarcaPageSize(Number(e.target.value));
              setMarcaPage(1);
            }}
            style={{ marginLeft: "1rem" }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </div>
        <br />
        <br />

        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Agregar nueva Marca:
        </h1>
        <FormAgregarMarca
          onMarcaAgregada={(nuevaMarca) => {
            setMarcas((prev) => [...prev, nuevaMarca]);
          }}
        />
        <br />
        <br />

        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />

        <br />

        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Relaciones entre Categorías y Marcas
        </h1>
        <h1
          style={{
            fontSize: "1.0rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Categorías y Marcas deben estar relacionadas para el correcto
          funcionamiento de los filtros de búsqueda de productos.
        </h1>
        <br />
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Todas las relaciones categoría-marca:
        </h1>
        <div
          style={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          {relaciones.map((rel) => (
            <CardCategoriaMarca
              key={rel.id}
              categoriaMarca={rel}
              onEliminar={() => handleEliminarRelacion(rel)}
            />
          ))}
        </div>
        <div style={{ margin: "1rem 0" }}>
          <button
            disabled={relacionPage === 1}
            onClick={() => setRelacionPage((prev) => prev - 1)}
          >
            Anterior
          </button>
          <span style={{ margin: "0 1rem" }}>
            Página {relacionPage} de{" "}
            {Math.ceil(totalRelaciones / relacionPageSize)}
          </span>
          <button
            disabled={relacionPage * relacionPageSize >= totalRelaciones}
            onClick={() => setRelacionPage((prev) => prev + 1)}
          >
            Siguiente
          </button>
          <select
            value={relacionPageSize}
            onChange={(e) => {
              setRelacionPageSize(Number(e.target.value));
              setRelacionPage(1);
            }}
            style={{ marginLeft: "1rem" }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </div>
        <br />
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Agregar nueva Relación:
        </h1>
        <FormAgregarRelacion
          categorias={categorias}
          marcas={marcas}
          onRelacionAgregada={(nuevaRelacion) => {
            setRelaciones((prev) => [...prev, nuevaRelacion]);
          }}
        />
        <br />
        <br />

        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />

        <br />
        <h1
          style={{
            fontSize: "2.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8px #ffe066",
            alignSelf: "center",
          }}
        >
          Usuarios
        </h1>
        <h1
          style={{
            fontSize: "1.3rem",
            fontWeight: "bold",
            color: "#bfa100",
            marginBottom: "1.5rem",
            letterSpacing: "2px",
            textShadow: "0 2px 8pxrgb(230, 190, 27)",
            alignSelf: "center",
          }}
        >
          Todos los usuario:
        </h1>
        <div
          style={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          {usuarios.map((usuario) => (
            <CardUsuario
              key={usuario.id}
              usuario={usuario}
              onEliminar={handleEliminarUsuario}
            />
          ))}
        </div>
        <div style={{ margin: "1rem 0" }}>
          <button
            disabled={usuarioPage === 1}
            onClick={() => setUsuarioPage((prev) => prev - 1)}
          >
            Anterior
          </button>
          <span style={{ margin: "0 1rem" }}>
            Página {usuarioPage} de {Math.ceil(totalUsuarios / usuarioPageSize)}
          </span>
          <button
            disabled={usuarioPage * usuarioPageSize >= totalUsuarios}
            onClick={() => setUsuarioPage((prev) => prev + 1)}
          >
            Siguiente
          </button>
          <select
            value={usuarioPageSize}
            onChange={(e) => {
              setUsuarioPageSize(Number(e.target.value));
              setUsuarioPage(1);
            }}
            style={{ marginLeft: "1rem" }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </div>
        <br />
        <br />
        <hr
          style={{
            width: "80%",
            border: "none",
            borderTop: "2px solid rgb(30, 30, 29)",
            margin: "2rem auto",
          }}
        />
        <br />
      </div>
    </>
  );
};

export default Panel;
