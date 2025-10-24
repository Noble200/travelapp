#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script FINAL para poblar la base de datos de Allva
TOTALMENTE ADAPTADO A LA ESTRUCTURA REAL
"""

import psycopg2
import hashlib
import uuid
from datetime import datetime
import sys

# ============================================
# CONFIGURACIÓN
# ============================================

DB_CONFIG = {
    'host': 'switchyard.proxy.rlwy.net',
    'port': 55839,
    'database': 'railway',
    'user': 'postgres',
    'password': 'ysTQxChOYSWUuAPzmYQokqrjpYnKSGbk'  # ⚠️ CAMBIAR
}

print("=" * 60)
print("🚀 ALLVA DATABASE SEED SCRIPT - VERSIÓN FINAL")
print("=" * 60)

# ============================================
# DATOS
# ============================================

COMERCIOS = [
    {
        'nombre': 'Allva Travel SRL',
        'nombre_srl': 'Allva Travel Sociedad de Responsabilidad Limitada',
        'direccion': 'Av. Rivadavia 1234, CABA, Argentina',
        'telefono': '+54 11 4567-8901',
        'email': 'contacto@allvatravel.com',
        'pais': 'Argentina',
        'comision': 2.5
    }
]

LOCALES = [
    {'codigo': 'CENTRAL', 'nombre': 'Casa Central', 'direccion': 'Av. Rivadavia 1234', 'comercio_id': None},
    {'codigo': 'BELGRANO', 'nombre': 'Sucursal Belgrano', 'direccion': 'Av. Cabildo 2500', 'comercio_id': None},
    {'codigo': 'PALERMO', 'nombre': 'Sucursal Palermo', 'direccion': 'Av. Santa Fe 3200', 'comercio_id': None},
]

ROLES = [
    {'nombre': 'Administrador', 'descripcion': 'Acceso total al sistema'},
    {'nombre': 'Gerente', 'descripcion': 'Gestión de local y reportes'},
    {'nombre': 'Empleado', 'descripcion': 'Operaciones básicas del día a día'},
]

USUARIOS = [
    {
        'numero': '1001',
        'nombre': 'Juan',
        'apellidos': 'Pérez',
        'password': 'Admin123!',
        'email': 'juan.perez@allvatravel.com',
        'telefono': '+54 11 1234-5678',
        'rol_nombre': 'Administrador',
        'flotante': False,
        'local_codigo': 'CENTRAL'
    },
    {
        'numero': '1002',
        'nombre': 'María',
        'apellidos': 'González',
        'password': 'Usuario123!',
        'email': 'maria.gonzalez@allvatravel.com',
        'telefono': '+54 11 2345-6789',
        'rol_nombre': 'Empleado',
        'flotante': False,
        'local_codigo': 'CENTRAL'
    },
    {
        'numero': '1003',
        'nombre': 'Carlos',
        'apellidos': 'Rodríguez',
        'password': 'Usuario123!',
        'email': 'carlos.rodriguez@allvatravel.com',
        'telefono': '+54 11 3456-7890',
        'rol_nombre': 'Empleado',
        'flotante': True,  # Usuario flotante NO tiene local fijo
        'local_codigo': None
    },
    {
        'numero': '9999',
        'nombre': 'Test',
        'apellidos': 'User',
        'password': 'Test1234!',
        'email': 'test@allvatravel.com',
        'telefono': '+54 11 9999-9999',
        'rol_nombre': 'Empleado',
        'flotante': False,
        'local_codigo': 'CENTRAL'
    }
]

# ============================================
# FUNCIONES
# ============================================

def hashear_password(password):
    return hashlib.sha256(password.encode()).hexdigest()

def conectar_db():
    try:
        conn = psycopg2.connect(**DB_CONFIG)
        print("✅ Conexión exitosa\n")
        return conn
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

def ejecutar_query(conn, query, params=None, fetch=False):
    try:
        cursor = conn.cursor()
        cursor.execute(query, params)
        
        if fetch:
            result = cursor.fetchone() if 'RETURNING' in query.upper() else cursor.fetchall()
            cursor.close()
            return result
        else:
            cursor.close()
            return True
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

# ============================================
# POBLACIÓN
# ============================================

def poblar_comercios(conn):
    print("📦 Poblando comercios...")
    
    query = """
    INSERT INTO comercios (
        nombre_comercio, nombre_srl, direccion_central, 
        numero_contacto, mail_contacto, pais,
        porcentaje_comision_divisas, activo, fecha_registro
    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s)
    RETURNING id_comercio;
    """
    
    comercio_id = None
    for comercio in COMERCIOS:
        params = (
            comercio['nombre'],
            comercio['nombre_srl'],
            comercio['direccion'],
            comercio['telefono'],
            comercio['email'],
            comercio['pais'],
            comercio['comision'],
            True,
            datetime.now()
        )
        
        result = ejecutar_query(conn, query, params, fetch=True)
        if result:
            comercio_id = result[0]
            print(f"  ✅ {comercio['nombre']} (ID: {comercio_id})")
    
    conn.commit()
    return comercio_id

def poblar_locales(conn, comercio_id):
    print("\n🏪 Poblando locales...")
    
    query = """
    INSERT INTO locales (
        id_comercio, codigo_local, nombre_local, direccion,
        activo, fecha_creacion
    ) VALUES (%s, %s, %s, %s, %s, %s)
    RETURNING id_local;
    """
    
    local_map = {}
    for local in LOCALES:
        params = (
            comercio_id,
            local['codigo'],
            local['nombre'],
            local['direccion'],
            True,
            datetime.now()
        )
        
        result = ejecutar_query(conn, query, params, fetch=True)
        if result:
            local_id = result[0]
            local_map[local['codigo']] = local_id
            print(f"  ✅ {local['nombre']} - {local['codigo']} (ID: {local_id})")
    
    conn.commit()
    return local_map

def poblar_roles(conn):
    print("\n👥 Poblando roles...")
    
    query = """
    INSERT INTO roles (
        nombre_rol, descripcion, fecha_creacion
    ) VALUES (%s, %s, %s)
    RETURNING id_rol;
    """
    
    rol_map = {}
    for rol in ROLES:
        params = (
            rol['nombre'],
            rol['descripcion'],
            datetime.now()
        )
        
        result = ejecutar_query(conn, query, params, fetch=True)
        if result:
            rol_id = result[0]
            rol_map[rol['nombre']] = rol_id
            print(f"  ✅ {rol['nombre']} (ID: {rol_id})")
    
    conn.commit()
    return rol_map

def poblar_permisos_modulos(conn, comercio_id):
    print("\n🧩 Asignando permisos de módulos...")
    
    query = """
    INSERT INTO permisos_modulos (
        id_comercio, modulo_divisas, modulo_pack_alimentos,
        modulo_billetes_avion, modulo_pack_viajes, fecha_asignacion
    ) VALUES (%s, %s, %s, %s, %s, %s);
    """
    
    params = (
        comercio_id,
        True,  # divisas
        True,  # alimentos
        True,  # billetes avión
        True,  # pack viajes
        datetime.now()
    )
    
    if ejecutar_query(conn, query, params):
        print(f"  ✅ Todos los módulos activados")
    
    conn.commit()

def poblar_usuarios(conn, comercio_id, local_map, rol_map):
    print("\n👤 Poblando usuarios...")
    
    query = """
    INSERT INTO usuarios (
        id_comercio, id_local, id_rol, numero_usuario,
        nombre, apellidos, correo, telefono, password_hash,
        es_flotante, idioma, activo, primer_login,
        intentos_fallidos, fecha_creacion
    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
    RETURNING id_usuario;
    """
    
    usuarios_creados = []
    for usuario in USUARIOS:
        password_hash = hashear_password(usuario['password'])
        
        # Si es flotante, id_local debe ser NULL
        if usuario['flotante']:
            local_id = None
        else:
            local_id = local_map.get(usuario['local_codigo'])
        
        rol_id = rol_map.get(usuario['rol_nombre'])
        
        params = (
            comercio_id,
            local_id,  # NULL para flotantes
            rol_id,
            usuario['numero'],
            usuario['nombre'],
            usuario['apellidos'],
            usuario['email'],
            usuario['telefono'],
            password_hash,
            usuario['flotante'],
            'es',
            True,
            False,
            0,
            datetime.now()
        )
        
        result = ejecutar_query(conn, query, params, fetch=True)
        if result:
            usuario_id = result[0]
            usuarios_creados.append({
                'id': usuario_id,
                'numero': usuario['numero'],
                'nombre': f"{usuario['nombre']} {usuario['apellidos']}",
                'password': usuario['password'],
                'local': usuario['local_codigo'] or 'FLOTANTE'
            })
            flotante_str = " (FLOTANTE)" if usuario['flotante'] else ""
            print(f"  ✅ {usuario['nombre']} {usuario['apellidos']} (#{usuario['numero']}){flotante_str}")
            print(f"     👉 Password: {usuario['password']}")
    
    conn.commit()
    return usuarios_creados

def crear_dispositivo_autorizado(conn, usuarios_creados):
    print("\n💻 Creando dispositivo autorizado...")
    
    query = """
    INSERT INTO dispositivos_autorizados (
        id_usuario, uuid_dispositivo, mac_address, nombre_dispositivo,
        sistema_operativo, navegador, dispositivo_tipo,
        autorizado, activo, fecha_registro
    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
    """
    
    test_user = next((u for u in usuarios_creados if u['numero'] == '9999'), None)
    
    if test_user:
        params = (
            test_user['id'],
            uuid.uuid4(),
            'AA:BB:CC:DD:EE:FF',
            'PC-TESTING',
            'Windows 11',
            'Chrome',
            'Desktop',
            True,
            True,
            datetime.now()
        )
        
        if ejecutar_query(conn, query, params):
            print(f"  ✅ Dispositivo autorizado para test_user")
    
    conn.commit()

def crear_configuracion_seguridad(conn, comercio_id):
    print("\n🔒 Creando configuración de seguridad...")
    
    query = """
    INSERT INTO configuracion_seguridad (
        id_comercio, password_min_length, password_require_uppercase,
        password_require_numbers, password_require_special_chars,
        login_max_intentos, login_bloqueo_minutos,
        sesion_duracion_horas, fecha_creacion
    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
    """
    
    params = (
        comercio_id,
        8,
        True,
        True,
        True,
        5,
        15,
        8,
        datetime.now()
    )
    
    if ejecutar_query(conn, query, params):
        print(f"  ✅ Configuración de seguridad creada")
    
    conn.commit()

# ============================================
# MAIN
# ============================================

def main():
    conn = conectar_db()
    if not conn:
        sys.exit(1)
    
    try:
        comercio_id = poblar_comercios(conn)
        if not comercio_id:
            raise Exception("No se pudo crear el comercio")
            
        local_map = poblar_locales(conn, comercio_id)
        rol_map = poblar_roles(conn)
        poblar_permisos_modulos(conn, comercio_id)
        usuarios_creados = poblar_usuarios(conn, comercio_id, local_map, rol_map)
        crear_dispositivo_autorizado(conn, usuarios_creados)
        crear_configuracion_seguridad(conn, comercio_id)
        
        print("\n" + "=" * 60)
        print("✅ BASE DE DATOS POBLADA EXITOSAMENTE")
        print("=" * 60)
        
        print("\n📊 RESUMEN:")
        print(f"  • Comercios: {len(COMERCIOS)}")
        print(f"  • Locales: {len(LOCALES)}")
        print(f"  • Roles: {len(ROLES)}")
        print(f"  • Usuarios: {len(USUARIOS)}")
        
        print("\n🔐 CREDENCIALES DE PRUEBA:")
        print("-" * 60)
        for usuario in usuarios_creados:
            print(f"\n  Usuario #{usuario['numero']}: {usuario['nombre']}")
            print(f"  📧 Password: {usuario['password']}")
            print(f"  🏪 Local: {usuario['local']}")
            
        print("\n💡 PARA PROBAR EL LOGIN:")
        print("  Usuario: 9999")
        print("  Password: Test1234!")
        print("  Local: CENTRAL")
        
        print("\n" + "=" * 60)
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        import traceback
        traceback.print_exc()
        conn.rollback()
    finally:
        conn.close()
        print("\n🔌 Conexión cerrada")

if __name__ == "__main__":
    main()