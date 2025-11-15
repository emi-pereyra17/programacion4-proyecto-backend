import React, { useState, useEffect } from "react";
import { toast } from "react-toastify";
import ValidationsForms from "../Validations/ValidationsForms";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

const FormAgregarMarca = ({ onMarcaAgregada }) => {
  const [nombre, setNombre] = useState("");
  const [paisId, setPaisId] = useState("");
  const [paises, setPaises] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errores, setErrores] = useState({});

  useEffect(() => {
    fetch(`${API_URL}/paises`)
      .then((res) => res.json())
      .then((data) => setPaises(data.paises || []));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const erroresVal = ValidationsForms({ nombre }, "marca");
    if (Object.keys(erroresVal).length > 0) {
      setErrores(erroresVal);
      toast.error(erroresVal.nombre || "Corrige los errores");
      return;
    }
    if (!paisId) {
      setErrores((prev) => ({ ...prev, paisId: "Debes seleccionar un país" }));
      toast.error("Debes seleccionar un país");
      return;
    }
    setErrores({});
    setLoading(true);

    try {
      const res = await fetch(`${API_URL}/marcas`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({ Nombre: nombre, PaisId: Number(paisId) }),
      });
      if (res.ok) {
        toast.success("Marca agregada correctamente");
        setNombre("");
        setPaisId("");
        if (onMarcaAgregada) {
          const data = await res.json();
          const marcaAgregada =
            data.marca || data.categoria || data.nuevaMarca || data;
          onMarcaAgregada(marcaAgregada);
        }
      } else {
        toast.error("No se pudo agregar la marca");
      }
    } catch {
      toast.error("Error de conexión al agregar marca");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        background: "#fffde7",
        padding: "1.5rem 2rem",
        borderRadius: "1rem",
        boxShadow: "0 2px 12px 0 rgba(191,161,0,0.08)",
        maxWidth: "800px",
        width: "600px",
        margin: "0 auto",
      }}
    >
      <label
        htmlFor="nombre"
        style={{
          color: "#222",
          fontWeight: "bold",
          marginBottom: "0.5rem",
          fontSize: "1.1rem",
        }}
      >
        Nombre de la marca:
      </label>
      <input
        id="nombre"
        name="nombre"
        value={nombre}
        onChange={(e) => setNombre(e.target.value)}
        style={{
          marginBottom: "0.5rem",
          padding: "0.5rem",
          background: "#fff",
          color: "#222",
          border: "1px solid #bfa100",
          borderRadius: "6px",
          width: "100%",
        }}
        disabled={loading}
        autoComplete="off"
      />
      {errores.nombre && (
        <p
          style={{ color: "red", marginTop: "-0.3rem", marginBottom: "0.5rem" }}
        >
          {errores.nombre}
        </p>
      )}

      {/* Select de países */}
      <label
        htmlFor="paisId"
        style={{
          color: "#222",
          fontWeight: "bold",
          marginBottom: "0.5rem",
          fontSize: "1.1rem",
        }}
      >
        País de la marca:
      </label>
      <select
        id="paisId"
        name="paisId"
        value={paisId}
        onChange={(e) => setPaisId(e.target.value)}
        style={{
          marginBottom: "0.5rem",
          padding: "0.5rem",
          background: "#fff",
          color: "#222",
          border: "1px solid #bfa100",
          borderRadius: "6px",
          width: "100%",
        }}
        disabled={loading}
      >
        <option value="">Selecciona un país</option>
        {paises.map((pais) => (
          <option key={pais.id} value={pais.id}>
            {pais.nombre}
          </option>
        ))}
      </select>
      {errores.paisId && (
        <p
          style={{ color: "red", marginTop: "-0.3rem", marginBottom: "0.5rem" }}
        >
          {errores.paisId}
        </p>
      )}

      <button
        type="submit"
        disabled={loading}
        style={{
          backgroundColor: "#FFD700",
          color: "#000",
          border: "1px solid #bfa100",
          fontWeight: "bold",
          fontSize: "1rem",
          padding: "0.5rem 2rem",
          borderRadius: "8px",
          cursor: loading ? "not-allowed" : "pointer",
          marginTop: "0.5rem",
        }}
      >
        {loading ? "Agregando..." : "Agregar Marca"}
      </button>
    </form>
  );
};

export default FormAgregarMarca;