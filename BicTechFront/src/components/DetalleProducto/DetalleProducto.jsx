import React, { useContext } from "react";
import "./DetalleProducto.css";
import Button from "react-bootstrap/Button";
import { AuthContext } from "../../context/AuthContext";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import { CarritoContext } from "../../context/CarritoContext";

const DetalleProducto = ({
  producto,
  marcaSeleccionada,
  categoriaSeleccionada,
  onClose,
}) => {
  const { usuario } = useContext(AuthContext);
  const navigate = useNavigate();
  const { agregarAlCarrito } = useContext(CarritoContext);

  const handleBtnComprarClick = async (e) => {
    if (!usuario) {
      e.preventDefault();
      toast.error("Debes iniciar sesión para comprar.");
      navigate("/login");
    } else {
      await agregarAlCarrito(producto.id, 1);
      toast.success("Producto agregado al carrito!");
    }
  };

  if (!producto) return null;

  return (
    <div className="detalle-overlay">
      <div className="detalle-contenido">
        <button className="cerrar-btn" onClick={onClose}>
          ✕
        </button>
        <div className="detalle-body">
          <img
            src={producto.imagenUrl}
            alt={producto.nombre}
            className="detalle-imagen"
          />
          <h2 className="detalle-titulo">{producto.nombre}</h2>
          <p className="detalle-descripcion">
            <strong>Descripción:</strong> {producto.descripcion}
          </p>
          <div className="precio-comprar-container">
            <h2 className="precio">Precio: ${producto.precio}</h2>
            {producto.stock <= 0 ? (
              <div
                style={{
                  background:
                    "linear-gradient(90deg, #ff1744 0%, #ff616f 100%)",
                  color: "#fff",
                  padding: "0.5rem 1.2rem",
                  borderRadius: "8px",
                  fontWeight: "bold",
                  fontSize: "1.1rem",
                  marginTop: "12px",
                  marginBottom: "12px",
                  textAlign: "center",
                  boxShadow: "0 2px 8px rgba(255,23,68,0.18)",
                  border: "2px solid #fff",
                  letterSpacing: "1px",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: "0.5rem",
                  textShadow: "1px 1px 4px #b71c1c",
                }}
              >
                <span style={{ fontSize: "1.3rem" }}>⚠️</span>
                Sin stock
              </div>
            ) : (
              <Button
                type="button"
                onClick={handleBtnComprarClick}
                style={{
                  marginBottom: "10px",
                  backgroundColor: "#FFD700",
                  color: "#000",
                  border: "1px solid #000",
                  fontWeight: "bold",
                  height: "fit-content",
                  fontSize: "1.5rem",
                  padding: "1rem 6rem",
                  borderRadius: "10px",
                }}
              >
                Comprar +
              </Button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DetalleProducto;
