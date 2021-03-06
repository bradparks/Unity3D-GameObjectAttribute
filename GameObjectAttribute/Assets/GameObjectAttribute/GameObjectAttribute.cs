﻿using GOA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GOA {
    public static class MemberInfoExtensions {

        public static bool CanWrite( this MemberInfo info ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    return true;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    return p.CanWrite;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return false;
            }
        }

        public static Type GetMemberType( this MemberInfo info ) {
            switch ( info.MemberType ) {
                case MemberTypes.Event:
                    var e = info as EventInfo;
                    return e.EventHandlerType;
                case MemberTypes.Field:
                    var f = info as FieldInfo;
                    return f.FieldType;
                case MemberTypes.Method:
                    var m = info as MethodInfo;
                    return m.ReturnType;
                case MemberTypes.Property:
                    var p = info as PropertyInfo;
                    return p.PropertyType;
                case MemberTypes.Constructor:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return null;
            }
        }

        public static void SetValue( this MemberInfo info, object obj, object value ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    var f = ( info as FieldInfo );
                    f.SetValue( obj, value );
                    break;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    p.SetValue( obj, value, null );
                    break;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    break;
            }
        }
    }
}

public static class GOAExtensions {

    private static Dictionary<Type, List<MemberInfo>> typeMembers = new Dictionary<Type, List<MemberInfo>>();

    private const string MISSING = "GameObject Finder: Unable to find {0}";
    private const string MISSING_ERROR = "GameObject Finder: Unable to find {0}, disabling {1} on {2}";
    private const string NO_WRITE = "GameObject Finder: Unable to write {0} on {1}";
    private const string NO_WRITE_ERROR = "GameObject Finder: Unable to write {0} on {1}, disabling {2}";

    public static void FindGameObjects( this MonoBehaviour behaviour ) {
        var bGameObject = behaviour.gameObject;
        var bType = behaviour.GetType();
        var cType = typeof( GameObjectAttribute );
        var gType = typeof( GameObject );
        List<MemberInfo> members;

        if ( typeMembers.ContainsKey( bType ) ) {
            members = typeMembers[bType];
        } else {
            members = bType.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( m =>
                    ( m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property )
                    && m.GetMemberType() == gType
                    && m.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

            members.OrderBy( m => m.MemberType ).ThenBy( m => m.Name );
            typeMembers.Add( bType, members );
        }

        foreach ( var item in members ) {
            var attribute = item.GetCustomAttributes( cType, true )[0] as GameObjectAttribute;

            var gameObject = GameObject.Find( attribute.Name );
            if ( gameObject == null ) {
                if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( bGameObject, MISSING_ERROR, attribute.Name, bType.Name, bGameObject.name );
                } else {
                    Debug.LogWarningFormat( bGameObject, MISSING, attribute.Name );
                }
            } else {
                if ( item.CanWrite() ) {
                    item.SetValue( behaviour, gameObject );
                } else {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( bGameObject, NO_WRITE_ERROR, item.Name, bType.Name, bGameObject.name );
                    } else {
                        Debug.LogWarningFormat( bGameObject, NO_WRITE, item.Name, bType.Name );
                    }
                }
            }
        }
    }
}

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
sealed class GameObjectAttribute : Attribute {

    public readonly string Name;
    public readonly bool DisableComponentOnError;

    public GameObjectAttribute( string name ) {
        Name = name;
        DisableComponentOnError = false;
    }

    public GameObjectAttribute( string name, bool disableComponentOnError ) {
        Name = name;
        DisableComponentOnError = disableComponentOnError;
    }
}