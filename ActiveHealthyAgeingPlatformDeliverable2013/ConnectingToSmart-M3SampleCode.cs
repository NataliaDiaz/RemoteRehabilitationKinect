using System;

public class M3Test: iKPIC_subscribeHandler
{
	public M3Test()
	{

        //Example of use for connecting to the Smart-M3 semantic RDF store
        //Examples show how to join and leave the space, how to insert, query and subscribe to specific data changes.
        //Notes: for floating points to be entered into the ontology, use ’.’.
        //Loading ontology programmatically from M3 wrappers does not work, should be entered from WebSIBExplorer (running explorer.py and through127.0.0.1:5000)


	}

    static void Main(string[] args)
    {
        //M3 CONNECTION
        //SSAP_XMLTools M3 = new SSAP_XMLTools("SitStand Node", "SmartSpace");
        string host2 = "130.232.85.58"; // Stefan "dodge.abo.fi"
        string hostAtom = "192.168.11.47";  // Atom board. Web-SIB-Explorer is also installed in the atom box, and should be available through http://192.168.11.47:5000/
        string hostLaptop = "192.168.11.38"; // in Stefan Laptop
        //   Insert in SPARQL query: PREFIX aha: <http://www.semanticweb.org/ontologies/2013/7/17/AHA.owl#>  //<http://users.abo.fi/ndiaz/public/AHA.owl#>   <= IRI (Protege refactoring to new IRI does not update all indexes).
        //  http://www.semanticweb.org/ontologies/2013/7/17/
        string pr =  "http://www.semanticweb.org/ontologies/2013/7/17/AHA.owl#"; // Equivalent to: aha:_ //ontology prefix
        // To access the WebSIBExplorer, run ESLab-Web-SIB-Explorer>explorer.py and go to: http://127.0.0.1:5000/

        KPICore.KPICore M3 = new KPICore.KPICore(hostAtom, 10010, "X");//, "SitStand");

        string query1 = "SELECT ?calendar1 ?user0  WHERE { ?user0 a " + pr + "User .  ?user0 " + pr + "hasName \"Natalia\"^^xsd:string. ?user0 " + pr + "hasCalendar ?calendar1 .}";
        string query2= "SELECT * WHERE {?s ?p ?o.}";
        string subQuery3 = "SELECT *  WHERE { ?session a " + pr + "SitStandSession.  ?session ?hasProperty ?property .}"; //"2005-02-28T00:00:00Z"^^xsd:dateTime ;
        

        ///TEST
        ///
        System.Console.WriteLine("--- Query1 " + query1);
        System.Console.WriteLine("--- Query2 " + query2);
        System.Console.WriteLine("--- Query3 float " + subQuery3);

        System.Console.WriteLine("--- Joining Smart Space X " + M3.join());// + M3.isJoinConfirmed(a));
        System.Console.WriteLine("--- Inserting OWL ontology " + M3.insertOWL(Path.Combine(Environment.CurrentDirectory, @"data\AHA.owl"))); // Only works from WebSIBExplorer

        //Graph as Arraylist of string[4]={s,p,o,o_type}   
        //Each RDF triple is represented using a string[4] datatype, where:
        //string[0] = subject
        //string[1] = predicate
        //string[2] = object
        //string[3] = object type ["uri"|"literal"]
        // Insert
        string[] cuatriple = new string[4] { pr + "SitStandSession" + DateTime.Now.ToString(), "a", pr + "SitStandSession", "uri" };
        string[] cuatriple2 = new string[4] { pr + "Natalia", pr + "executesSitStandSession", (pr + "SitStandSession" + DateTime.Now.ToString()), "literal" };
        ArrayList triple = new ArrayList();
        triple.Add(cuatriple);
        ArrayList triple2 = new ArrayList();
        triple2.Add(cuatriple2);
        System.Console.WriteLine("--- Inserting " + M3.insert(triple));
        System.Console.WriteLine("--- Inserting " + M3.insert(triple2));

        username = "Natalia";
        sessionDuration = 0.9f;
        avgSecondsToSit = 8.8f;
        avgSecondsToStand = 8.6f;
        nSits = 7;
        SitStandExerciseSession session = new SitStandExerciseSession(("SitStandSession" + username + DateTime.Now.ToString()), username, DateTime.Now, sessionDuration, nSits, avgSecondsToSit, avgSecondsToStand);
        sessions.Add(session);
        foreach (SitStandExerciseSession sitSession in sessions)
        {
            string[] cuatriple_ = new string[4] { pr + session.SessionName, "a", pr + "SitStandSession", "uri" };
            string[] cuatriple2_ = new string[4] { pr + session.Username, pr + "executesSitStandSession", pr + session.SessionName, "literal" };
            string[] cuatriple3 = new string[4] { pr + session.SessionName, pr + "hasStartDateTime", DateTimeToOWLDateTimeStr(session.StartDateTime), "literal" };
            string[] cuatriple4 = new string[4] { pr + session.SessionName, pr + "hasDuration", session.Duration.ToString(), "literal" };
            string[] cuatriple5 = new string[4] { pr + session.SessionName, pr + "consistsOfNSits", session.NSits.ToString(), "literal" };
            string[] cuatriple6 = new string[4] { pr + session.SessionName, pr + "tookAvgSecondsToSit", session.AvgSecondsToSit.ToString(), "literal" };
            string[] cuatriple7 = new string[4] { pr + session.SessionName, pr + "tookAvgSecondsToStand", session.AvgSecondsToStand.ToString(), "literal" };

            ArrayList triple_ = new ArrayList();
            triple_.Add(cuatriple);
            ArrayList triple2_ = new ArrayList();
            triple2_.Add(cuatriple2);
            ArrayList triple3 = new ArrayList();
            triple3.Add(cuatriple3);
            ArrayList triple4 = new ArrayList();
            triple4.Add(cuatriple4);
            ArrayList triple5 = new ArrayList();
            triple5.Add(cuatriple5);
            ArrayList triple6 = new ArrayList();
            triple6.Add(cuatriple6);
            ArrayList triple7 = new ArrayList();
            triple7.Add(cuatriple7);
            System.Console.WriteLine("--- Inserting " + M3.insert(triple_));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple2_));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple3));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple4));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple5));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple6));
            System.Console.WriteLine("--- Inserting " + M3.insert(triple7));
        }

        ////TEST

        SPARQLResults queryResults = new SPARQLResults();
        queryResults = M3.getSPARQLResults(query1);

        System.Console.WriteLine("--- Querying SPARQL " + queryResults);
        System.Console.WriteLine("--- Querying SPARQL " + M3.getSPARQLResults(query2)); //querySPARQL(query2));
        string results = M3.querySPARQL(query2);
        System.Console.WriteLine("--- Querying SPARQL " + results);
        System.Console.WriteLine("--- Transforming DateTime " + DateTime.Now + " to OWL DateTime: " + DateTimeToOWLDateTimeDatatypeStr(DateTime.Now));
        System.Console.WriteLine("--- Transforming TimeSpan " + (DateTime.UtcNow - DateTime.Now).Duration() + " to OWL DateTime: " + TimeSpanToOWLDateTimeDatatypeStr(DateTime.UtcNow - DateTime.Now) + " in Seconds: " + (DateTime.UtcNow - DateTime.Now).Duration().TotalSeconds);
        System.Console.WriteLine("--- Transforming int " + 8 + " to OWL int: " + IntToOWLIntDatatypeStr(8));
        System.Console.WriteLine("--- Transforming string natalia" + " to OWL string: " + StringToOWLStringDatatypeStr("natalia"));
        System.Console.WriteLine("--- Transforming bool " + (8 == 8) + " to OWL bool: " + BooleanToOWLBooleanDatatypeStr((8 == 8)));
        System.Console.WriteLine("--- Transforming float " + 0.45f + " to OWL float: " + FloatToOWLFloatDatatypeStr(0.45f));
        /////TEST


        // Subscription
        SPARQLResults subscripResults = new SPARQLResults();
        //iKPIC_subscribeHandler subEventHandler = new iKPIC_subscribeHandler();
        //string subscriptionID = M3.subscribeSPARQL(subQuery3, kpic_SIBEventHandlerSPARQL, subscripResults);  // HOW TO CALL IT?
        //System.Console.WriteLine("--- Subscribing " + subscriptionID);
        
        // Unsubscribe
        //System.Console.WriteLine("--- Unsubscribing " + M3.unsubscribe(subscriptionID));            

        // Leave
        System.Console.WriteLine("--- Leaving the Smart Space X " + M3.leave()); //when cleaning (closing) the program
    }

}
