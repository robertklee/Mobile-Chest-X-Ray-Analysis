"""Module containing all the code for the models supported by the model conversion scripts"""

from keras.models import Model
from keras.models import load_model
from keras.layers import Dense
from keras_contrib.applications.densenet import DenseNetImageNet121

def build_from_file(model_path):
    """Builds a Keras Model from the model defined in the file at the provided path and with weights loaded from the same file
    
    Args:
        model_path: str
            path to the Keras model file, this file must have been saved using Model.save() NOT Model.save_weights(), since the latter
            does not actually save the model structure

    Returns:
        The Keras Model loaded from the file
    """

    return load_model(model_path)

def build_densenet_model():
    """Returns a Keras instance of the DenseNet121 model with no pre-loaded weights"""

    return DenseNetImageNet121(
            input_shape=(224, 224, 3),
            include_top=False,
            weights=None,
            pooling='avg')

def build_chestxray_model(weights_path = None):
    """Builds the ChestXRay model which consists of the DenseNet121 model with an appended, 14-element, fully connected layer
    
    Args:
        weights_path: str
            * path to the file containing the weights to initialize the ChestXRay model
            * the weights in this file must be for the ChestXRay model architecture
            * the file can have been created using either Model.save() or Model.save_weights()
            * if None is provided the model will not be initialized with any weights

    Returns:
        The constructed ChestXRay model
    """

    # Construct the original DenseNet model
    base_model = build_densenet_model()

    # Create a Dense layer for the 14-way classification
    x = base_model.output
    predictions = Dense(14, activation='sigmoid')(x)

    # Construct the final model (Base Model + Dense Layer)
    model = Model(inputs=base_model.input, outputs=predictions)

    # Optionally load the saved weights into the model
    if weights_path is not None:
        model.load_weights(weights_path)

    return model

def build_chestxray_cam_model(chestxray_weights_path = None):
    """Builds the ChestXRay CAM model (a variant of the ChestXRay model without the final two layers)

    The two layers that are removed are the pooling layer at the end of the DenseNet121 and the final Dense 14-element layer
    This model exposes activation_121 as an output, a 7x7x1024 tensor containing the data that can be used to generate Class Activation Maps (CAMs)

    Args:
        chestxray_weights_path: str
            * path to the file containing the weights to initialize the ChestXRay model
            * the weights in this file should be for the full ChestXRay model (including the final two layers)
              as the weights will be loaded before the final two layers are removed
            * the file can have been created using either Model.save() or Model.save_weights()
            * if None is provided the model will not be initialized with any weights

    Returns:
        The constructed ChestXRay CAM model
    """

    # Construct the original DenseNet model
    base_model = build_chestxray_model()

    # Optionally load the saved weights into the model
    if chestxray_weights_path is not None:
        base_model.load_weights(chestxray_weights_path)

    # Ignore the final 2 layers (Pooling and Dense)
    model = Model(inputs=base_model.input, outputs=base_model.layers[-3].output)

    return model

"""Dictionary of functions that can build a Keras model and optionally load weights from a provided file path into it

This dictionary of functions are the model architectures that the model conversion scripts will support

If you want the model conversion python scripts to support additional model architectures, add an entry to this dictionary in the format:
    "<key>" : <function>,   <key> is a string identifier for your model architecture
                            <function> is a callable that accepts an optional string path to a weights file and returns a Keras Model object

After adding an entry to the dictionary, you can use that architecture as follows:

    python <model conversion script> --model_arch <key> (other options for that script)

"""
MODEL_ARCHITECTURES = {
    "from-file": build_from_file,
    "chestxray": build_chestxray_model,
    "chestxray-cam": build_chestxray_cam_model
}

def available_architectures():
    """Returns the list of available model architectures (i.e. a list of strings can be passed to load(model_arch, model_file_path))"""
    return list(MODEL_ARCHITECTURES.keys())

def load(model_arch, weights_file_path = None):
    """Loads a Keras Model using the function for the indicated model architecture

    Args:
        model_arch: str
            one of the keys returned from available_architectures()
        weights_file_path: str
            path to a weights file to load into the model

    Returns:
        the loaded Keras model
    """
    return MODEL_ARCHITECTURES[model_arch](weights_file_path)