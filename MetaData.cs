using System;
using System.Collections;
using System.Collections.Generic; 
namespace WeeboxSync {
    public class MetaData{

        // Basic elements present in a valid MetadaTa 
/*- <properties version="1.0">
  <entry key="bundle.submission.date">2011-05-13T13:21:35.17Z</entry> 
  <entry key="bundle.submitter.address">127.0.0.1</entry> 
  <entry key="bundle.submitter.host">127.0.0.1</entry> 
  <entry key="bundle.owner">fabiim</entry> 
  <entry key="bundle.remote.user" /> 
  <entry key="bundle.new.version.of">4F543C7E8E2D9FD52AB900590323E594</entry> 
  <entry key="bundle.data.files.id">ECABBA3A927B7F3B0716B26AAA72D303,D2F57E7A55E7E97EFEB26C95DA84058C</entry> 
  <entry key="bundle.id">8E239724985D402FD33D4C0C9BD452B4</entry> 
  <entry key="bundle.submitter.port">54096</entry> 
  <entry key="bundle.submitter.agent">Jakarta Commons-HttpClient/3.1</entry> 
  <entry key="bundle.permission.readers">fabiim</entry> 
  <entry key="bundle.referer" /> 
  <entry key="bundle.permission.modifiers">fabiim</entry> 
  <entry key="bundle.number.data.files">2</entry> 
  <entry key="bundle.active">true</entry> 
  <entry key="bundle.version">3</entry> 
  </properties>
 */
        
       public  Dictionary<String,String> keyValueData = new Dictionary<String,String>();
        public MetaData(){

        }
    }
}
