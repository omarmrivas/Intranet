module Weka

let algo () =
    let reader = new java.io.BufferedReader( new java.io.FileReader("glass.arff") )
    let data = weka.core.Instances reader
    data.setClassIndex(data.numAttributes() - 1)
    data

let options () =
    let scheme = new weka.classifiers.meta.AttributeSelectedClassifier()
    scheme.setOptions(weka.core.Utils.splitOptions("-E \"weka.attributeSelection.CfsSubsetEval -P 1 -E 1\" -S \"weka.attributeSelection.BestFirst -D 1 -N 5\" -W weka.classifiers.trees.J48 -- -C 0.25 -M 2"))
    scheme

(*
let updateHeaderNumeric (atts : weka.core.FastVector) materia periodos =
    // Clave Profesor
    let profesoresVals = weka.core.FastVector()
    periodos |> List.map (BaseDatos.obtener_claves_profesores materia)
             |> List.concat
             |> set
             |> Set.toList
             |> List.map string
             |> List.iter (fun clave -> profesoresVals.addElement(clave))
    atts.addElement( weka.core.Attribute(materia + "_profesor", profesoresVals) )
    // C1
    atts.addElement( weka.core.Attribute(materia + "_c1") )
    // I1
    atts.addElement( weka.core.Attribute(materia + "_i1") )
    // C2
    atts.addElement( weka.core.Attribute(materia + "_c2") )
    // I2
    atts.addElement( weka.core.Attribute(materia + "_i2") )
    // C3
    atts.addElement( weka.core.Attribute(materia + "_c3") )
    // I3
    atts.addElement( weka.core.Attribute(materia + "_i3") )
    // Efinal
    atts.addElement( weka.core.Attribute(materia + "_efinal") )
    // Final
    atts.addElement( weka.core.Attribute(materia + "_final") )
    // Inasistencias
    atts.addElement( weka.core.Attribute(materia + "_inasistencias") )
    // Estatus
    atts.addElement( weka.core.Attribute(materia + "_estatus") )
    ()*)


let createArff () =
    // 1. set up attributes
    let atts = weka.core.FastVector()
    // - numeric
    atts.addElement( weka.core.Attribute("att1") )
    // - nominal
    let attVals = weka.core.FastVector()
    [0..4] |> List.iter (fun i -> attVals.addElement("val" + string (i+1)))
    atts.addElement( weka.core.Attribute("att2", attVals) )
    // - string
    let nullvector : weka.core.FastVector = null
    atts.addElement( weka.core.Attribute("att3", nullvector) )
    // - date
    atts.addElement( weka.core.Attribute("att4", "yyyy-MM-dd") )
    // - relational
    let attsRel = weka.core.FastVector()
    // -- numeric
    attsRel.addElement( weka.core.Attribute("att5.1") )
    // -- nominal
    let attValsRel = weka.core.FastVector()
    [0..4] |> List.iter (fun i -> attValsRel.addElement("val5." + string (i+1)))
    attsRel.addElement( weka.core.Attribute("att5.2", attValsRel) )
    let dataRel = weka.core.Instances("att5", attsRel, 0)
    atts.addElement( weka.core.Attribute("att5", dataRel, 0) )

    // 2. create Instances object
    let data = weka.core.Instances("MyRelation", atts, 0)

    // 3. fill with data
    // first instance
    let vals = [| java.lang.Math.PI
                  (float)(attVals.indexOf("val3"))
                  (float)(data.attribute(2).addStringValue("This is a string!"))
                  data.attribute(3).parseDate("2001-11-09") |]

    let dataRel = weka.core.Instances(data.attribute(4).relation(), 0)
    // -- first instance
    let valsRel = [| java.lang.Math.PI + 1.0
                     (float)(attValsRel.indexOf("val5.3")) |]
    dataRel.add( weka.core.DenseInstance(1.0, valsRel) ) |> ignore
    // -- second instance
    let valsRel = [| java.lang.Math.PI + 2.0
                     (float)(attValsRel.indexOf("val5.2")) |]
    dataRel.add( weka.core.DenseInstance(1.0, valsRel) ) |> ignore
    let vals = Array.append vals [| (float)(data.attribute(4).addRelation(dataRel)) |]

    // add
    data.add( weka.core.DenseInstance(1.0, vals) ) |> ignore

    // second instance
    let vals = [| java.lang.Math.E
                  (float)(attVals.indexOf("val1"))
                  (float)(data.attribute(2).addStringValue("And another one!"))
                  (data.attribute(3).parseDate("2000-12-01")) |]

    // - relational
    let dataRel = weka.core.Instances( data.attribute(4).relation(), 0 )

    // -- first instance
    let valsRel = [| java.lang.Math.E + 1.0
                     (float)(attValsRel.indexOf("val5.4")) |]
    dataRel.add( weka.core.DenseInstance(1.0, valsRel) ) |> ignore

    // -- second instance
    let valsRel = [| java.lang.Math.E + 2.0
                     (float)(attValsRel.indexOf("val5.1")) |]
    dataRel.add( weka.core.DenseInstance(1.0, valsRel) ) |> ignore

    let vals = Array.append vals [| (float)(data.attribute(4).addRelation(dataRel)) |]

    data.add( weka.core.DenseInstance(1.0, vals) ) |> ignore

    data.toString()

