using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip; // Biblioteca para zipar os arquivos

//Bibliotecas de Importação Gdal
using OSGeo.OGR;
using OSGeo.OSR;

namespace GDAL_JsonToShapeFile
{
    class GdalUtilities
    {
            //Configura o Gdal
            public GdalUtilities()
            {
                GdalConfiguration.ConfigureGdal();
                GdalConfiguration.ConfigureOgr();
            }

            public bool convertJsonToShapeFile(string jsonFilePath, string shapeFilePath)
            {
                try
                {
                    //Parte do código responsável por ABRIR O JSON COM O GDAL e pegar a camada(layer)===============
                    Driver jsonFileDriver = Ogr.GetDriverByName("GeoJSON");
                    DataSource jsonFile = Ogr.Open(jsonFilePath, 0);

                    //Se o json estiver null retorna false 
                    if (jsonFile == null)
                    {
                        return false;
                    }

                    //Se não
                    //Parte do código que CRIA O SHAPEFILE COM O GDAL================================================
                    string filesPathName = shapeFilePath.Substring(0, shapeFilePath.Length - 4);

                    //Se ele já existir é removido
                    removeShapeFileIfExists(filesPathName);

                    //Layer json
                    Layer jsonLayer = jsonFile.GetLayerByIndex(0);

                    Driver esriShapeFileDriver = Ogr.GetDriverByName("ESRI Shapefile");
                    DataSource shapeFile = esriShapeFileDriver.CreateDataSource(shapeFilePath, new string[] { });

                    //Layer do shp
                    Layer shplayer = shapeFile.CreateLayer(jsonLayer.GetName(), jsonLayer.GetSpatialRef(), jsonLayer.GetGeomType(), new string[] { });

                    //Copia dados e propriedades de um Json para um ShapeFile
                    //cria campos (propriedades) em uma nova camada
                    Feature jsonFeature = jsonLayer.GetNextFeature();
                    for (int i = 0; i < jsonFeature.GetFieldCount(); i++)
                    {                                      //Verifica se o nome do arquivo shape não ultrapassa o limite de 10 caracteres 
                        FieldDefn fieldDefn = new FieldDefn(getValidFieldName(jsonFeature.GetFieldDefnRef(i)), jsonFeature.GetFieldDefnRef(i).GetFieldType());
                        shplayer.CreateField(fieldDefn, 1);
                    }

                    while (jsonFeature != null)
                    {
                        Geometry geometry = jsonFeature.GetGeometryRef();
                        Feature shpFeature = createGeometryFromGeometry(geometry, shplayer, jsonLayer.GetSpatialRef());

                        //Copia os valores para cada campo
                        for (int i = 0; i < jsonFeature.GetFieldCount(); i++)
                        {
                            if (FieldType.OFTInteger == jsonFeature.GetFieldDefnRef(i).GetFieldType())
                            {
                                shpFeature.SetField(getValidFieldName(jsonFeature.GetFieldDefnRef(i)), jsonFeature.GetFieldAsInteger(i));
                            }
                            else if (FieldType.OFTReal == jsonFeature.GetFieldDefnRef(i).GetFieldType())
                            {
                                shpFeature.SetField(getValidFieldName(jsonFeature.GetFieldDefnRef(i)), jsonFeature.GetFieldAsDouble(i));
                            }
                            else
                            {
                                shpFeature.SetField(getValidFieldName(jsonFeature.GetFieldDefnRef(i)), jsonFeature.GetFieldAsString(i));
                            }
                        }

                        shplayer.SetFeature(shpFeature);

                        jsonFeature = jsonLayer.GetNextFeature();

                    }

                    shapeFile.Dispose();

                    //Caso seja necessário zipar a pasta
                    string zipName = filesPathName + ".zip";
                    CompressToZipFile(new List<string>() { shapeFilePath, filesPathName + ".dbf", filesPathName + ".prj", filesPathName + ".shx" }, zipName);

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Erro encontrado no método de conversão " + e.Message);
                }
            }

            //Método remove o arquivo dependendo de sua string com sua extensão 
            private void removeShapeFileIfExists(string filesPathName)
            {
                removeFileIfExists(filesPathName + ".shp");
                removeFileIfExists(filesPathName + ".shx");
                removeFileIfExists(filesPathName + ".prj");
                removeFileIfExists(filesPathName + ".zip");
            }

            //Método quando invocado verifica a existencia do arquivo com a extensão, se o arquivo já existir é removido
            public static bool removeFileIfExists(string filePath)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }

            //Verifica se o nome do shapefile ultrapassa o limite de 10 caracteres 
            private string getValidFieldName(FieldDefn fieldDefn)
            {
                string fieldName = fieldDefn.GetName();
                return fieldName.Length > 10 ? fieldName.Substring(0, 10) : fieldName;
            }

            private Feature createGeometryFromGeometry(Geometry geometry, Layer layer, SpatialReference reference)
            {
                Feature feature = new Feature(layer.GetLayerDefn());

                string wktgeometry = "";
                geometry.ExportToWkt(out wktgeometry);
                Geometry newGeometry = Geometry.CreateFromWkt(wktgeometry);
                newGeometry.AssignSpatialReference(reference);
                newGeometry.SetPoint(0, geometry.GetX(0), geometry.GetY(0), 0);

                feature.SetGeometry(newGeometry);
                layer.CreateFeature(feature);

                return feature;
            }

            public static void CompressToZipFile(List<string> files, string zipPath)
            {
                using (ZipFile zip = new ZipFile())
                {
                    foreach (string file in files)
                    {
                        zip.AddFile(file, "");
                    }
                    zip.Save(zipPath);
                }
            }

        }
    }



    